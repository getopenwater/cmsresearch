import { BaseComponent } from "wysiwyg/view/components/base/BaseComponent";

export abstract class BubbleMenuBaseComponent extends BaseComponent {

   constructor(protected container: HTMLElement, protected controllerIdentifier: string) {
      super(container, controllerIdentifier);
   }

   protected initialize(): void { }

   protected appendToContainer(): void {
      // required empty
   }

   public getBubbleMenu(): HTMLElement {
      return this.element;
   }
}