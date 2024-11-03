import { IEditorState } from "wysiwyg/interfaces/IEditorState";

export interface IEditorView {
   appendEditor(element: Element): void;
   updateView(newState: IEditorState, previousState: IEditorState | null): void;
   destroy(): void;
}