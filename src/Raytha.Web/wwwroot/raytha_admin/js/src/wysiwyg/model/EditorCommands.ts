import { Editor } from '@tiptap/core';
import { IEditorCommands } from 'wysiwyg/interfaces/IEditorCommands';

export class EditorCommands implements IEditorCommands {

   constructor(private editor: Editor) { }

   newDocument(): boolean {
      return this.editor.chain().focus().clearContent().run();
   }

   undo(): boolean {
      return this.editor.chain().focus().undo().run();
   }

   redo(): boolean {
      return this.editor.chain().focus().redo().run();
   }

   copy(): boolean {
      return this.editor.chain().focus().copy().run();
   }

   cut(): boolean {
      return this.editor.chain().focus().cutContent().run();
   }

   paste(): boolean {
      return this.editor.chain().focus().paste(false).run();
   }

   pasteAsText(): boolean {
      throw new Error('Method not implemented.');
   }

   selectAll(): boolean {
      return this.editor.chain().focus().selectAll().run();
   }

   setSearchTerm(searchTerm: string): boolean {
      return this.editor.chain().focus().setSearchTerm(searchTerm).run();
   }

   setReplaceTerm(replaceTerm: string): boolean {
      return this.editor.chain().focus().setReplaceTerm(replaceTerm).run();
   }

   nextSearchResult(): number {
      this.editor.chain().focus().nextSearchResult().run();

      return this.editor?.storage?.searchAndReplace.resultIndex;
   }

   previousSearchResult(): number {
      this.editor.chain().focus().previousSearchResult().run();

      return this.editor?.storage?.searchAndReplace.resultIndex;
   }

   setCaseSensitive(caseSensitive: boolean): boolean {
      return this.editor.chain().focus().setCaseSensitive(caseSensitive).run();
   }

   replace(): boolean {
      return this.editor.chain().focus().replace().run();
   }

   replaceAll(): boolean {
      return this.editor.chain().focus().replaceAll().run();
   }

   setContent(content: string): boolean {
      return this.editor.commands.setContent(content, false, {
         preserveWhitespace: "full",
         
      });
   }

   toggleInvisibleCharacters(): boolean {
      return this.editor.chain().focus().toggleInvisibleCharacters().run();
   }

   toggleTextBlocks(): boolean {
      throw new Error('Method not implemented.');
   }

   toggleSpellcheck(): boolean {
      throw new Error('Method not implemented.');
   }

   insertLink(url: string, text: string, title: string, openInNewWindow: boolean): boolean {
      return this.editor.chain().focus().extendMarkRange('link').setLink({ href: url, target: openInNewWindow ? "_blank" : "_self", title: title })
         .command(({ tr }) => {
            tr.insertText(text)
            return true
         }).run()
   }

   insertImageByLink(url: string, altText: string | null): boolean {
      return this.editor.chain().focus().setImage({ src: url, alt: altText ?? 'altText' }).run();
   }

   insertYoutubeVideo(url: string, width: number, height: number): boolean {
      return this.editor.chain().focus().setYoutubeVideo({ src: url, width: width, height: height, }).run();
   }

   insertTable(rows: number, cols: number): boolean {
      return this.editor.chain().focus().insertTable({ rows: rows, cols: cols, withHeaderRow: true }).run();
   }

   addRowBefore(): boolean {
      return this.editor.chain().focus().addRowBefore().run();
   }

   addRowAfter(): boolean {
      return this.editor.chain().focus().addRowAfter().run();
   }

   addColumnBefore(): boolean {
      return this.editor.chain().focus().addColumnBefore().run();
   }

   addColumnAfter(): boolean {
      return this.editor.chain().focus().addColumnAfter().run();
   }

   deleteRow(): boolean {
      return this.editor.chain().focus().deleteRow().run();
   }

   deleteColumn(): boolean {
      return this.editor.chain().focus().deleteColumn().run();
   }

   deleteTable(): boolean {
      return this.editor.chain().focus().deleteTable().run();
   }

   mergeCells(): boolean {
      return this.editor.chain().focus().mergeCells().run();
   }

   splitCell(): boolean {
      return this.editor.chain().focus().splitCell().run();
   }

   toggleHeaderRow(): boolean {
      return this.editor.chain().focus().toggleHeaderRow().run();
   }

   toggleHeaderColumn(): boolean {
      return this.editor.chain().focus().toggleHeaderColumn().run();
   }

   toggleHeaderCell(): boolean {
      return this.editor.chain().focus().toggleHeaderCell().run();
   }

   fixTable(): boolean {
      return this.editor.chain().focus().fixTables().run();
   }

   //todo curr method -> insertContent
   insertSpecialCharacter(character: string): boolean {
      return this.editor.chain().focus().insertContent(character).run();
   }

   insertHorizontalRule(): boolean {
      return this.editor.chain().focus().setHorizontalRule().run();
   }

   insertPageBreak(): boolean {
      throw new Error('Method not implemented.');
   }

   insertNonbreakingSpace(): boolean {
      return this.editor.chain().focus().insertContent('\u00A0').run();
   }

   //todo curr method -> insertContent
   insertDateTime(datetime: string): boolean {
      return this.editor.chain().focus().insertContent(datetime).run();
   }

   toggleBold(): boolean {
      return this.editor.chain().focus().toggleBold().run();
   }

   toggleItalic(): boolean {
      return this.editor.chain().focus().toggleItalic().run();
   }

   toggleUnderline(): boolean {
      return this.editor.chain().focus().toggleUnderline().run();
   }

   toggleStrike(): boolean {
      return this.editor.chain().focus().toggleStrike().run();
   }

   toggleSuperscript(): boolean {
      return this.editor.chain().focus().toggleSuperscript().run();
   }

   toggleSubscript(): boolean {
      return this.editor.chain().focus().toggleSubscript().run();
   }

   toggleCode(): boolean {
      return this.editor.chain().focus().toggleCode().run();
   }

   insertParagraph(): boolean {
      return this.editor.chain().focus().setParagraph().run();
   }

   insertHeading(level: 1 | 2 | 3 | 4 | 5 | 6): boolean {
      return this.editor.chain().focus().unsetFontSize().setHeading({ level: level }).run();
   }

   setFontFamily(fontFamily: string): boolean {
      return this.editor.chain().focus().setFontFamily(fontFamily).run();
   }

   setFontSize(fontSize: string): boolean {
      return this.editor.chain().focus().setFontSize(fontSize).run();
   }

   setTextAlign(textAlign: string): boolean {
      return this.editor.chain().focus().setTextAlign(textAlign).run();
   }

   setLineHeight(lineHeight: string): boolean {
      return this.editor.chain().focus().setLineHeight(String(lineHeight)).run();
   }

   setTextColor(color: string): boolean {
      return this.editor.chain().focus().setColor(color).run();
   }

   setBackgroundColor(color: string): boolean {
      return this.editor.chain().focus().setHighlight({ color: color }).run();
   }

   clearFormatting(): boolean {
      return this.editor.chain().focus().unsetAllMarks().run();
   }

   addIndent(): boolean {
      return this.editor.chain().focus().indent().run();
   }

   removeIndent(): boolean {
      return this.editor.chain().focus().outdent().run();
   }

   toggleBlockquote(): boolean {
      return this.editor.chain().focus().toggleBlockquote().run();
   }

   toggleCodeBlock(): boolean {
      return this.editor.chain().focus().toggleCodeBlock().run();
   }

   toggleBulletList(): boolean {
      return this.editor.chain().focus().toggleBulletList().run();
   }

   toggleOrderedList(): boolean {
      return this.editor.chain().focus().toggleOrderedList().run();
   }

   splitListItem(): boolean {
      return this.editor.chain().focus().splitListItem("listItem").run();
   }

   sinkListItem(): boolean {
      return this.editor.chain().focus().sinkListItem("listItem").run();
   }

   liftListItem(): boolean {
      return this.editor.chain().focus().liftListItem("listItem").run();
   }
}