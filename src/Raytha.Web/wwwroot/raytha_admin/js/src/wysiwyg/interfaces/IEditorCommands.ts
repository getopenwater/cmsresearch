export interface IEditorCommands {

   //file
   newDocument(): boolean;

   //edit
   undo(): boolean;
   redo(): boolean;
   copy(): boolean;
   cut(): boolean;
   paste(): boolean;
   pasteAsText(): boolean;
   selectAll(): boolean;
   setSearchTerm(searchTerm: string): boolean;
   setReplaceTerm(replaceTerm: string): boolean;
   nextSearchResult(): number;
   previousSearchResult(): number;
   setCaseSensitive(caseSensitive: boolean): boolean;
   replace(): boolean;
   replaceAll(): boolean;

   //view
   setContent(content: string): boolean;
   toggleInvisibleCharacters(): boolean;
   toggleTextBlocks(): boolean;
   toggleSpellcheck(): boolean;

   //insert
   insertLink(url: string, text: string, title: string, openInNewWindow: boolean): boolean;
   insertImageByLink(url: string, altText: string | null): boolean;
   insertYoutubeVideo(url: string, width: number, height: number): boolean;
   insertTable(rows: number, cols: number): boolean;
   addRowBefore(): boolean;
   addRowAfter(): boolean;
   addColumnBefore(): boolean;
   addColumnAfter(): boolean;
   deleteRow(): boolean;
   deleteColumn(): boolean;
   deleteTable(): boolean;
   mergeCells(): boolean;
   splitCell(): boolean;
   toggleHeaderRow(): boolean;
   toggleHeaderColumn(): boolean;
   toggleHeaderCell(): boolean;
   fixTable(): boolean;
   insertSpecialCharacter(character: string): boolean;
   insertHorizontalRule(): boolean;
   insertPageBreak(): boolean;
   insertNonbreakingSpace(): boolean;
   insertDateTime(datetime: string): boolean;

   //format
   toggleBold(): boolean;
   toggleItalic(): boolean;
   toggleUnderline(): boolean;
   toggleStrike(): boolean;
   toggleSuperscript(): boolean;
   toggleSubscript(): boolean;
   toggleCode(): boolean;
   insertParagraph(): boolean;
   insertHeading(level: number): boolean;
   setFontFamily(fontFamily: string): boolean;
   setFontSize(fontSize: string): boolean;
   setTextAlign(textAlign: string): boolean;
   setLineHeight(lineHeight: string): boolean;
   setTextColor(color: string): boolean;
   setBackgroundColor(color: string): boolean;
   clearFormatting(): boolean;
   addIndent(): boolean;
   removeIndent(): boolean;
   toggleBlockquote(): boolean;
   toggleCodeBlock(): boolean;
   toggleBulletList(): boolean;
   toggleOrderedList(): boolean;
   splitListItem(): boolean;
   sinkListItem(): boolean;
   liftListItem(): boolean;
}