import { Extension } from '@tiptap/core';
import { Plugin, PluginKey } from '@tiptap/pm/state';
import { Decoration, DecorationSet } from '@tiptap/pm/view';
import { Node } from '@tiptap/pm/model'

declare module '@tiptap/core' {
  interface Commands<ReturnType> {
    invisibleCharacters: {
      toggleInvisibleCharacters: () => ReturnType,
    }
  }
}

export const InvisibleCharacters = Extension.create({
  name: 'invisibleCharacters',

  addOptions() {
    return {
      enabled: false,
    };
  },

  addCommands() {
    return {
      toggleInvisibleCharacters: () => ({ editor }) => {
        const enabled = this.options.enabled;
        this.options.enabled = !enabled;
        editor.view.dispatch(editor.state.tr);

        return true;
      },
    };
  },

  addProseMirrorPlugins() {
    return [
      new Plugin({
        key: new PluginKey('invisibleCharacters'),

        props: {
          decorations: (state) => {
            if (!this.options.enabled)
              return;

            const doc: Node = state.doc;
            const decorations: Decoration[] = [];

            doc.descendants((node, pos) => {
              if (node.type.name === 'paragraph') {
                decorations.push(Decoration.widget(pos + node.nodeSize - 1, () => {

                  const span = document.createElement('span');
                  span.className = 'invisible-paragraph';

                  return span;
                }));
              }

              if (node.type.name === 'text') {

                let position: number = 0;
                const text: string = node.text || '';

                for (const char of text) {
                  console.log(text);
                  if (char === ' ' || char === '\u00A0') {
                    decorations.push(Decoration.widget(pos + position, () => {
                      const span = document.createElement('span');
                      span.className = 'invisible-space';

                      return span;
                    }));
                  }

                  position += 1;
                }
              }

              if (node.type.name === 'hardBreak') {
                decorations.push(Decoration.widget(pos + node.nodeSize - 1, () => {
                  const span = document.createElement('span');
                  span.className = 'invisible-newline';

                  return span;
                }));
              }
            });

            return DecorationSet.create(doc, decorations);
          },
        },
      }),
    ];
  },
});
