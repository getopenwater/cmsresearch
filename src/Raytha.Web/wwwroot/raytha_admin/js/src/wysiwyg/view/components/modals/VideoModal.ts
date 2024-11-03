import { ModalBaseComponent } from "wysiwyg/view/components/modals/base/ModalBaseComponent";
import videoModalTemplate from "wysiwyg/view/components/modals/templates/videoModal.html";

export class VideoModal extends ModalBaseComponent {
   protected render(): HTMLElement {
      return this.createElementFromTemplate(videoModalTemplate);
   }

   protected initialize(): void {
      this.bindShowModal((event) => {
         (event.target as HTMLElement).querySelector('form')?.reset();
      });
   }
}