import { IEditorState } from "wysiwyg/interfaces/IEditorState";

export interface IEditorStateManager {
   subscribe(callback: (state: IEditorState, previousState: IEditorState | null) => void): () => void;
   unsubscribe(callback: (state: IEditorState, previousState: IEditorState | null) => void): void;
   forceUpdate(): void;
   getState(): IEditorState;
   destroy(): void;
}