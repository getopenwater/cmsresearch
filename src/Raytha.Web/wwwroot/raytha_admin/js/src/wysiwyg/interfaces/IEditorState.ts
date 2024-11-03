export interface IEditorState {
   marks: {
      bold: boolean;
      italic: boolean;
      underline: boolean;
      strike: boolean;
      superscript: boolean;
      subscript: boolean;
      code: boolean;
   };
   textStyle: {
      fontFamily: string | null;
      fontSize: string | null;
      color: string | null;
      lineHeight: string | null;
   }
   formats: {
      'paragraph': boolean;
      'heading1': boolean;
      'heading2': boolean;
      'heading3': boolean;
      'heading4': boolean;
      'heading5': boolean;
      'heading6': boolean;
   };
   textAlign: string;
   highlight: string | null;
   list: {
      ordered: boolean;
      bullet: boolean;
   },
   blockquote: boolean;
   codeBlock: boolean;
   invisibleCharacters: boolean;
   cursorPosition: string;
   words: string;
   searchResult: {
      total: number;
      index: number;
   }
}