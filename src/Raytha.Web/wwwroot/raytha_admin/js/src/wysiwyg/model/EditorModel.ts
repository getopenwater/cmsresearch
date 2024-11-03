import { Editor as TiptapEditor } from '@tiptap/core';
import { AnyExtension } from "@tiptap/core";

//Mark
import Bold from "@tiptap/extension-bold";
import Code from "@tiptap/extension-code";
import Highlight from "@tiptap/extension-highlight";
import Italic from "@tiptap/extension-italic";
import LinkExtension from "wysiwyg/model/extensions/link/index";
import Strike from "@tiptap/extension-strike";
import Subscript from "@tiptap/extension-subscript";
import Superscript from "@tiptap/extension-superscript";
import TextStyle from "@tiptap/extension-text-style";
import Underline from "@tiptap/extension-underline";

// Nodes
import Blockquote from "@tiptap/extension-blockquote";
import BulletList from "@tiptap/extension-bullet-list";
import CodeBlock from "@tiptap/extension-code-block";
import Document from "@tiptap/extension-document";
import Text from "@tiptap/extension-text";
import HardBreak from "@tiptap/extension-hard-break";
import Heading from "@tiptap/extension-heading";
import HorizontalRule from "@tiptap/extension-horizontal-rule";
import ListItem from "@tiptap/extension-list-item";
import OrderedList from "@tiptap/extension-ordered-list";
import Paragraph from "@tiptap/extension-paragraph";
import Table from "@tiptap/extension-table";
import TableCell from "@tiptap/extension-table-cell";
import TableHeader from "@tiptap/extension-table-header";
import TableRow from "@tiptap/extension-table-row";
import Youtube from "@tiptap/extension-youtube";
import Image from "@tiptap/extension-image";

// Functionally
import CharacterCount from "@tiptap/extension-character-count";
import Color from "@tiptap/extension-color";
import TextAlign from "@tiptap/extension-text-align";
import Typography from "@tiptap/extension-typography";
import History from "@tiptap/extension-history";
import FontFamily from "@tiptap/extension-font-family";
import BubbleMenu from "@tiptap/extension-bubble-menu";
import FontSize from "wysiwyg/model/extensions/fontSize/index";
import CutCopyPaste from "wysiwyg/model/extensions/cutCopyPaste/index";
import SearchAndReplace from "wysiwyg/model/extensions/searchAndReplace/index";
import LineHeight from "wysiwyg/model/extensions/lineHeight/index";
import Indent from "wysiwyg/model/extensions/indent/index";
import InvisibleCharacters from "wysiwyg/model/extensions/invisibleCharacters/index";

import { IEditor } from 'wysiwyg/interfaces/IEditor';
import { IEditorCommands } from 'wysiwyg/interfaces/IEditorCommands';
import { IEditorStateManager } from 'wysiwyg/interfaces/IEditorStateManager';
import { EditorCommands } from "wysiwyg/model/EditorCommands"
import { EditorStateManager } from "wysiwyg/model/EditorStateManager"

export class EditorModel implements IEditor {
   public readonly editor: TiptapEditor;
   public readonly commands: IEditorCommands;
   public readonly stateManager: IEditorStateManager;

   constructor(private tableBubbleMenu?: HTMLElement | null) {
      this.editor = new TiptapEditor({
         extensions: this.getExtensions(),
         editorProps: {
            attributes: {
               class: 'p-2',
            },
            
         },
      });

      this.commands = new EditorCommands(this.editor);
      this.stateManager = new EditorStateManager(this.editor);
   }

   private getExtensions(): Array<any> {
      return [
         Bold,
         Code,
         Highlight.configure({
            multicolor: true,
         }),
         Italic,
         LinkExtension.configure({
            HTMLAttributes: {
               class: "link-primary",
            },
         }),
         Strike,
         Subscript,
         Superscript,
         TextStyle,
         Underline,
         Blockquote,
         BulletList,
         CodeBlock,
         Document,
         Text,
         HardBreak,
         Heading,
         HorizontalRule,
         ListItem,
         OrderedList,
         Paragraph,
         Table.configure({
            resizable: true,
         }),
         TableCell,
         TableHeader,
         TableRow,
         Youtube,
         Image.configure({ inline: true }),
         CharacterCount,
         Color,
         TextAlign.configure({
            types: ["heading", "paragraph"],
         }),
         Typography,
         History,
         FontFamily,
         BubbleMenu.configure({
            pluginKey: 'tableBubbleMenu',
            element: this.tableBubbleMenu,
            shouldShow: ({ editor }) => {
               return editor.isActive('table') || editor.isActive('tableCell') || editor.isActive('tableHeader') || editor.isActive('tableRow');
            },
            tippyOptions: {
               placement: 'top',
               interactive: true,
            }
         }),
         FontSize,
         CutCopyPaste,
         SearchAndReplace,
         LineHeight,
         Indent,
         InvisibleCharacters,
      ];
   }

   public configureExtensions<T extends AnyExtension>(name: string, optionsUpdater: (currentOptions: T['options']) => Partial<T['options']>) {
      const extension = this.editor.extensionManager.extensions.find(
         ext => ext.name === name
      );

      if (extension) {
         try {
            const currentOptions = extension.options;
            const newOptions = optionsUpdater(currentOptions);
            extension.configure(newOptions);
         } catch (error) {
            console.error(`Error updating extension "${name}":`, error);
         }
      };
   }

   public destroy(): void {
      if (this.editor) {
         this.stateManager.destroy();
         this.editor.destroy();
      }
   }
}