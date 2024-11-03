import { BaseComponent } from "wysiwyg/view/components/base/BaseComponent";
import { Modal } from "bootstrap"

export abstract class ModalBaseComponent extends BaseComponent {
   private modalInstance: Modal;

   constructor(container: HTMLElement, controllerIdentifier: string) {
      super(container, controllerIdentifier);
      this.modalInstance = Modal.getOrCreateInstance(this.element);
   }

   public bindShowModal(handler: (event: Event) => void): void {
      this.bindEvent(this.element, 'show.bs.modal', handler);
   }

   public bindHideModal(handler: (event: Event) => void): void {
      this.bindEvent(this.element, 'hide.bs.modal', handler);
   }

   public hide() {
      this.modalInstance.hide();
   }
}