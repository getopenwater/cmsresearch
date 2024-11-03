import { DropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/DropdownMenuBaseComponent";
import dateTimeTemplate from "wysiwyg/view/components/dropdownMenus/templates/dateTime.html";

export class DateTimeDropdownMenu extends DropdownMenuBaseComponent {
   private currentTimeElement: HTMLElement;
   private currentDateIsoElement: HTMLElement;
   private currentTime12Element: HTMLElement;
   private currentDateUsElement: HTMLElement;

   constructor(container: HTMLElement, controllerIdentifier: string) {
      super(container, controllerIdentifier);

      this.currentTimeElement = this.querySelector('[data-item-role="currentTime"]') as HTMLElement;
      this.currentDateIsoElement = this.querySelector('[data-item-role="currentDateIso"]') as HTMLElement;
      this.currentTime12Element = this.querySelector('[data-item-role="currentTime12"]') as HTMLElement;
      this.currentDateUsElement = this.querySelector('[data-item-role="currentDateUs"]') as HTMLElement;
   }

   protected render(): HTMLElement {
      return this.createElementFromTemplate(dateTimeTemplate);
   }

   protected initialize(): void {
      this.bindShowDropdown(() => {
         const now = new Date();

         this.currentTimeElement.textContent = now.toLocaleTimeString();
         this.currentDateIsoElement.textContent = now.toISOString().split('T')[0];

         this.currentTime12Element.textContent = now.toLocaleTimeString('en-US', {
            hour: 'numeric',
            minute: '2-digit',
            second: '2-digit',
            hour12: true
         });

         this.currentDateUsElement.textContent = now.toLocaleDateString('en-US', {
            month: '2-digit',
            day: '2-digit',
            year: 'numeric'
         });
      });
   }
}