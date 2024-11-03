import { BaseComponent } from "wysiwyg/view/components/base/BaseComponent";
import { Dropdown } from "bootstrap";

export abstract class DropdownMenuBaseComponent extends BaseComponent {
   protected appendToContainer(): void {
      this.container.appendChild(this.element);
      const dropdownTrigger = this.container.querySelector('[data-bs-toggle="dropdown"]');
      if (dropdownTrigger) {
         new Dropdown(dropdownTrigger);
      }
   }

   public bindShowDropdown(handler: (event: Event) => void): void {
      this.bindEvent(this.container, 'show.bs.dropdown', handler);
   }

   public hiddenDropdown(handler: (event: Event) => void): void {
      this.bindEvent(this.container, 'hidden.bs.dropdown', handler);
   }
}