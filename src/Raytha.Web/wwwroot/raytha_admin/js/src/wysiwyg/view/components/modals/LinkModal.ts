import { ModalBaseComponent } from "wysiwyg/view/components/modals/base/ModalBaseComponent";
import linkModalTemplate from "wysiwyg/view/components/modals/templates/linkModal.html";

export class LinkModal extends ModalBaseComponent {
   protected render(): HTMLElement {
      return this.createElementFromTemplate(linkModalTemplate);
   }

   protected initialize(): void {
       
   }
}