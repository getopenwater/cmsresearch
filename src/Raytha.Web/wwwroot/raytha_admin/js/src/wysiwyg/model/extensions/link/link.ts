import Link from '@tiptap/extension-link';

declare module '@tiptap/core' {
    interface Commands<ReturnType> {
      linkWithTitle: {
        /**
         * Set a link mark
         * @param attributes The link attributes
         * @example editor.commands.setLink({ href: 'https://tiptap.dev' })
         */
        setLink: (attributes: { href: string; target?: string | null; rel?: string | null; class?: string | null, title?: string | null }) => ReturnType
        /**
         * Toggle a link mark
         * @param attributes The link attributes
         * @example editor.commands.toggleLink({ href: 'https://tiptap.dev' })
         */
        toggleLink: (attributes: { href: string; target?: string | null; rel?: string | null; class?: string | null, title?: string | null }) => ReturnType
        /**
         * Unset a link mark
         * @example editor.commands.unsetLink()
         */
        unsetLink: () => ReturnType
      }
    }
  }

export const LinkExtension = Link.extend({
    addAttributes() {
        return {
            ...this.parent?.(),
            title: {
                default: null,
                parseHTML: element => element.getAttribute('title'),
                renderHTML: attributes => {
                    if (!attributes.title) {
                        return;
                    }

                    return {
                        title: attributes.title,
                    }
                }
            }
        }
    }
});