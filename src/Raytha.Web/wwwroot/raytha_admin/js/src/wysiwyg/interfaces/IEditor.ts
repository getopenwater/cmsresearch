import { Editor } from "@tiptap/core";
import { IEditorCommands } from "wysiwyg/interfaces/IEditorCommands";
import { IEditorStateManager } from "wysiwyg/interfaces//IEditorStateManager";

export interface IEditor {
    readonly editor: Editor;
    readonly commands: IEditorCommands;
    readonly stateManager: IEditorStateManager;

    destroy(): void;
}

