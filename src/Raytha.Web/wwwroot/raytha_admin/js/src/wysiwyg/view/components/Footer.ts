import footerTemplate from "wysiwyg/view/components/templates/footer.html";
import { UpdateableComponent } from "./base/UpdateableComponent";
import { UpdateableDropdownMenuBaseComponent } from "./dropdownMenus/base/UpdateableDropdownMenuBaseComponent";

export class Footer extends UpdateableComponent {
   protected render(): HTMLElement {
      return this.createElementFromTemplate(footerTemplate);
   }

   protected initialize(): void {}

   protected getComponents(): {
      dropdowns?: { [key: string]: { path: string; component: UpdateableDropdownMenuBaseComponent<any>; }; };
      buttons?: { [key: string]: { selector: string; path: string; activeClass?: string; }; };
      text?: { [key: string]: { selector: string; path: string; }; };
   } {
      return {
         text: {
            cursorPosition: {
               selector: '#cursorPosition',
               path: 'cursorPosition',
            },
            words: {
               selector: '#wordsCount',
               path: 'words',
            },
         },
      };
   }
}