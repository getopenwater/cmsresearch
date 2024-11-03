import { Extension } from '@tiptap/core'
import { DOMSerializer } from '@tiptap/pm/model'
import { EditorState } from '@tiptap/pm/state';

declare module "@tiptap/core" {
    interface Commands<ReturnType> {
        cutCopyPaste: {
            cutContent: () => ReturnType;
            copy: () => ReturnType;
            paste: (asText: boolean) => ReturnType;
        }
    }
}

export const CutCopyPaste = Extension.create({
    name: 'cutCopyPaste',
    addAttributes() {
        return {
            asText: {
                default: false,
            }
        }
    },
    addCommands() {
        const getSelectionContent = (state: EditorState) => {
            const { from, to } = state.selection;
            const slice = state.doc.slice(from, to);
            const serializer = DOMSerializer.fromSchema(state.schema);
            const fragment = serializer.serializeFragment(slice.content);

            const div = document.createElement('div');
            div.appendChild(fragment);
            const text = state.doc.textBetween(from, to);
            const html = div.innerHTML.trim();

            return { text, html };
        }

        const writeToClipboard = (text: string, html: string) => {
            const clipboardItem = new ClipboardItem({
                'text/plain': new Blob([text], { type: 'text/plain' }),
                'text/html': new Blob([html], { type: 'text/html' }),
            });

            navigator.clipboard.write([clipboardItem]);

            return navigator.clipboard.write([clipboardItem]);
        }

        return {
            cutContent: () => ({ state, dispatch }) => {
                if (!navigator.clipboard || !navigator.clipboard.write) {
                    console.error('Clipboard API not supported');
                    return false;
                }

                const content = getSelectionContent(state);
                writeToClipboard(content.text, content.html).catch(error => {
                    console.log(error);

                    return false;
                });

                if (dispatch) {
                    dispatch(state.tr.delete(state.selection.from, state.selection.to));
                }

                return true;
            },
            copy: () => ({ state }) => {
                if (!navigator.clipboard || !navigator.clipboard.write) {
                    console.error('Clipboard API not supported');
                    return false;
                }

                const content = getSelectionContent(state);
                writeToClipboard(content.text, content.html).catch(error => {
                    console.log(error);

                    return false;
                });

                return true;
            },
            paste: (asText) => ({ editor }) => {
                if (!navigator.clipboard || !navigator.clipboard.read) {
                    console.error('Clipboard API not supported');
                    return false;
                }

                navigator.clipboard.read().then(clipboardItems => {
                    clipboardItems.forEach(async item => {
                        const type = asText
                            ? "text/plain"
                            : "text/html";

                        const blob = await item.getType(type).catch(() => null);
                        if (blob) {
                            blob.text().then(text => {
                                editor.chain().focus().insertContent(text, {
                                    applyPasteRules: false,
                                }).run();;
                            })
                        }
                    });
                }).catch(error => {
                    console.error(error);

                    return false;
                });

                return true;
            }
        }
    },
})