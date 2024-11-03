import { BaseComponent } from "wysiwyg/view/components/base/BaseComponent";
import containerTemplate from "wysiwyg/view/components/templates/editorContainer.html";

export class EditorContainer extends BaseComponent {
   private _header!: HTMLElement;
   private _body!: HTMLElement;
   private _footer!: HTMLElement;

   public get header(): HTMLElement {
      return this._header;
   }

   public get body(): HTMLElement {
      return this._body;
   }

   public get footer(): HTMLElement {
      return this._footer;
   }

   protected render(): HTMLElement {
      return this.createElementFromTemplate(containerTemplate);
   }

   protected initialize(): void {
      this._header = this.querySelector('.card-header') as HTMLElement;
      this._body = this.querySelector('.card-body') as HTMLElement;
      this._footer = this.querySelector('.card-footer') as HTMLElement;
   }
}