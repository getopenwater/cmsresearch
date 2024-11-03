import { Editor } from "@tiptap/core";
import { debounce, throttle } from 'lodash';
import { IEditorState } from "wysiwyg/interfaces/IEditorState";
import { IEditorStateManager } from "wysiwyg/interfaces/IEditorStateManager";

export class EditorStateManager implements IEditorStateManager {
   private subscribers: Array<(newState: IEditorState, previousState: IEditorState | null) => void> = [];
   private currentState: IEditorState;
   private previousState: IEditorState | null;
   private lastContent: string;

   private updateEditorThrottle: ReturnType<typeof throttle>;
   private updateEditorDebounce: ReturnType<typeof debounce>;
   private updateSelectionThrottle: ReturnType<typeof throttle>;
   private onContentChangeCallback?: (content: string) => void;

   constructor(private editor: Editor) {
      this.initializeState();
      this.setupFunctions();
      this.setupEditorListeners();
   }

   public setContentChangeCallback(callback: (content: string) => void): void {
      this.onContentChangeCallback = callback;
   }

   private initializeState(): void {
      this.currentState = {
         marks: this.getActiveMarks(),
         textStyle: this.getActiveTextStyle(),
         formats: this.getActiveFormat(),
         textAlign: this.getActiveTextAlign(),
         highlight: this.getActiveHighlight(),
         list: this.getActiveList(),
         blockquote: this.editor.isActive('blockquote'),
         codeBlock: this.editor.isActive('codeBlock'),
         invisibleCharacters: this.editor.isActive('invisibleCharacters'),
         cursorPosition: this.getCursorPosition(),
         words: `${this.editor.storage.characterCount.words()} words`,
         searchResult: {
            total: 0,
            index: 0,
         }
      }

      this.previousState = null;
      this.lastContent = this.editor.getHTML();
   }

   private setupFunctions(): void {
      this.updateEditorThrottle = throttle(
         () => this.updateIfContentChanged(),
         500
      );

      this.updateEditorDebounce = debounce(
         () => this.updateIfContentChanged(),
         500
      );

      this.updateSelectionThrottle = throttle(
         () => this.updateEditorState(),
         500,
         { leading: true, trailing: true }
      );
   }

   private setupEditorListeners(): void {
      const onSelectionUpdated = () => {
         this.updateSelectionThrottle();
      };

      const onEditorUpdated = () => {
         this.updateEditorThrottle();
         this.updateEditorDebounce();
      }

      this.editor.on('update', onEditorUpdated);
      this.editor.on('selectionUpdate', onSelectionUpdated);
   }

   private updateIfContentChanged(): void {
      const newContent = this.editor.getHTML();
      if (this.lastContent === newContent) return;

      this.lastContent = newContent;
      this.updateEditorState();

      if (this.onContentChangeCallback) {
         this.onContentChangeCallback(newContent);
      }
   }

   private updateEditorState(): void {
      const newState = {
         marks: this.getActiveMarks(),
         textStyle: this.getActiveTextStyle(),
         formats: this.getActiveFormat(),
         textAlign: this.getActiveTextAlign(),
         highlight: this.getActiveHighlight(),
         list: this.getActiveList(),
         blockquote: this.editor.isActive('blockquote'),
         codeBlock: this.editor.isActive('codeBlock'),
         invisibleCharacters: this.editor.extensionManager.extensions.find(extension => extension.name === 'invisibleCharacters')?.options.enabled ?? false,
         cursorPosition: this.getCursorPosition(),
         words: `${this.editor.storage.characterCount.words()} words`,
         searchResult: this.getSearchResultTotal(),
      } as IEditorState;

      if (!this.isStateEqual(this.currentState, newState)) {
         this.previousState = { ...this.currentState };
         this.currentState = newState;
         this.notifySubscribers();
      }
   }

   private getSearchResultTotal(): IEditorState['searchResult'] {
      const { results, resultIndex } = this.editor?.storage?.searchAndReplace ?? {};
      const total = results.length;

      if (total === 0) {
         return {
            total: 0,
            index: 0
         };
      }

      return {
         total: total,
         index: resultIndex === 0 ? 1 : resultIndex + 1
      } as IEditorState['searchResult'];
   }

   private getActiveMarks(): IEditorState['marks'] {
      const editorFunctions = {
         bold: (editor: Editor): boolean => editor.isActive('bold'),
         italic: (editor: Editor): boolean => editor.isActive('italic'),
         underline: (editor: Editor): boolean => editor.isActive('underline'),
         strike: (editor: Editor): boolean => editor.isActive('strike'),
         superscript: (editor: Editor): boolean => editor.isActive('superscript'),
         subscript: (editor: Editor): boolean => editor.isActive('subscript'),
         code: (editor: Editor): boolean => editor.isActive('code'),
      };

      const marks: Record<string, boolean> = {};

      for (const [key, func] of Object.entries(editorFunctions)) {
         marks[key] = func(this.editor);
      }

      return marks as IEditorState['marks'];
   }

   private getActiveTextStyle(): IEditorState['textStyle'] {
      const attrs = this.editor.getAttributes('textStyle');
      const nodeAttrs = this.editor.getAttributes('paragraph') || this.editor.getAttributes('heading');

      return {
         fontFamily: attrs.fontFamily || 'Helvetica',
         fontSize: attrs.fontSize || '16px',
         color: attrs.color || '#000000',
         lineHeight: nodeAttrs.lineHeight || '1.4',
      } as IEditorState['textStyle'];
   }

   private getActiveFormat(): IEditorState['formats'] {
      const editorFunctions = {
         paragraph: (editor: Editor): boolean => editor.isActive('paragraph'),
         heading1: (editor: Editor): boolean => editor.isActive('heading', { level: 1 }),
         heading2: (editor: Editor): boolean => editor.isActive('heading', { level: 2 }),
         heading3: (editor: Editor): boolean => editor.isActive('heading', { level: 3 }),
         heading4: (editor: Editor): boolean => editor.isActive('heading', { level: 4 }),
         heading5: (editor: Editor): boolean => editor.isActive('heading', { level: 5 }),
         heading6: (editor: Editor): boolean => editor.isActive('heading', { level: 6 }),
      };

      const formats: Record<string, boolean> = {};
      for (const [key, func] of Object.entries(editorFunctions)) {
         formats[key] = func(this.editor);
      }

      return formats as IEditorState['formats'];
   }

   private getActiveTextAlign(): IEditorState['textAlign'] {
      const currentAlignment = ["left", "center", "right", "justify"].find((alignment) =>
         this.editor.isActive({ textAlign: alignment })
      );

      return currentAlignment || 'left';
   }

   private getActiveHighlight(): IEditorState['highlight'] {
      const attrs = this.editor.getAttributes('highlight');

      return attrs.color || "#000000";
   }

   private getActiveList(): IEditorState['list'] {
      return {
         ordered: this.editor.isActive('orderedList'),
         bullet: this.editor.isActive('bulletList'),
      };
   }

   private getCursorPosition(): IEditorState['cursorPosition'] {
      const { $from } = this.editor.state.selection;
      const path: string[] = [];
      const marksInText = new Set();

      for (let i = $from.depth; i >= 0; i--) {
         const node = $from.node(i);
         if (node.type.spec.toDOM) {
            const domSpec = node.type.spec.toDOM(node);
            const tagName = Array.isArray(domSpec) ? domSpec[0] : domSpec;

            if (tagName !== "div") {
               path.unshift(tagName);
            }
         }

         const marks = $from.marks();
         for (const mark of marks) {
            if (!marksInText.has(mark.type.name)) {
               marksInText.add(mark.type.name);
               if (mark.type.spec.toDOM) {
                  const domSpec = mark.type.spec.toDOM(mark, true);
                  const tagName = Array.isArray(domSpec) ? domSpec[0] : domSpec;
                  path.push(tagName);
               } else {
                  path.push(mark.type.name);
               }
            }
         }
      }

      return path.join(" > ");
   }

   private isStateEqual(oldState: IEditorState, newState: IEditorState): boolean {
      return JSON.stringify(oldState) === JSON.stringify(newState);
   }

   private notifySubscribers(): void {
      this.subscribers.forEach(callback => callback(this.currentState, this.previousState));
   }

   public subscribe(callback: (state: IEditorState, previousState: IEditorState | null) => void): () => void {
      this.subscribers.push(callback);
      callback(this.currentState, null);
      return () => this.unsubscribe(callback);
   }

   public unsubscribe(callback: (state: IEditorState, previousState: IEditorState) => void): void {
      this.subscribers = this.subscribers.filter(sub => sub !== callback);
   }

   public getState(): IEditorState {
      return { ...this.currentState };
   }

   public forceUpdate(): void {
      this.updateEditorState();
   }

   public destroy(): void {
      this.updateEditorThrottle.cancel();
      this.updateEditorDebounce.cancel();
      this.updateSelectionThrottle.cancel();
      this.onContentChangeCallback = undefined;
      this.subscribers = [];
      this.editor.off('update');
      this.editor.off('selectionUpdate');
   }
}