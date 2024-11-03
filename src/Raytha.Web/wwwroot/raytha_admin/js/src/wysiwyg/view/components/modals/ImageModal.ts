import { ModalBaseComponent } from "wysiwyg/view/components/modals/base/ModalBaseComponent";
import imageModalTemplate from "wysiwyg/view/components/modals/templates/imageModal.html";

export class ImageModal extends ModalBaseComponent {
   protected render(): HTMLElement {
      return this.createElementFromTemplate(imageModalTemplate);
   }

   protected initialize(): void {
       
   }
}