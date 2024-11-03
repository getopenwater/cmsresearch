import { ModalBaseComponent } from "wysiwyg/view/components/modals/base/ModalBaseComponent";
import sourceCodeModalTemplate from "wysiwyg/view/components/modals/templates/sourceCodeModal.html";

export class SourceCodeModal extends ModalBaseComponent {
   private _textarea: HTMLTextAreaElement | null;

   protected render(): HTMLElement {
      return this.createElementFromTemplate(sourceCodeModalTemplate);
   }

   protected initialize(): void {
      this._textarea = this.querySelector<HTMLTextAreaElement>('#sourceCodeTextarea');
   }

   public getTextArea(): HTMLTextAreaElement | null {
      return this._textarea;
   }
}