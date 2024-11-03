import { UpdateableComponent } from "wysiwyg/view/components/base/UpdateableComponent";
import { UpdateableDropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/UpdateableDropdownMenuBaseComponent"
import { TableDropdownMenu } from "wysiwyg/view/components/dropdownMenus/TableDropdownMenu";
import { DateTimeDropdownMenu } from "wysiwyg/view/components/dropdownMenus/DateTimeDropdownMenu"
import { FormatDropdownMenu } from "wysiwyg/view/components/dropdownMenus/FormatDropdownMenu"
import { FontFamilyDropdownMenu } from "wysiwyg/view/components/dropdownMenus/FontFamilyDropdownMenu"
import { FontSizeDropdownMenu } from "wysiwyg/view/components/dropdownMenus/FontSizeDropdownMenu"
import { TextAlignDropdownMenu } from "wysiwyg/view/components/dropdownMenus/TextAlignDropdownMenu"
import { LineHeightDropdownMenu } from "wysiwyg/view/components/dropdownMenus/LineHeightDropdownMenu"
import { TextColorDropdownMenu } from "wysiwyg/view/components/dropdownMenus/TextColorDropdownMenu"
import { BackgroundColorDropdownMenu } from "wysiwyg/view/components/dropdownMenus/BackgroundColorDropdownMenu"
import menubarTemplate from "wysiwyg/view/components/templates/menubar.html";

export class Menubar extends UpdateableComponent {
   private formatDropdownMenu: FormatDropdownMenu;
   private fontFamilyDropdownMenu: FontFamilyDropdownMenu;
   private fontSizeDropdownMenu: FontSizeDropdownMenu;
   private textAlignDropdownMenu: TextAlignDropdownMenu;
   private lineHeightDropdownMenu: LineHeightDropdownMenu;
   private textColorPalette: TextColorDropdownMenu;
   private backgroundColorPalette: BackgroundColorDropdownMenu;

   protected render(): HTMLElement {
      return this.createElementFromTemplate(menubarTemplate);
   }

   protected initialize(): void {
      const tableDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="table"]');
      if (tableDropdown) {
         new TableDropdownMenu(tableDropdown, this.controllerIdentifier);
      }

      const datetimeDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="datetime"]');
      if (datetimeDropdown) {
         new DateTimeDropdownMenu(datetimeDropdown, this.controllerIdentifier);
      }

      const formatDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="format"]');
      if (formatDropdown) {
         this.formatDropdownMenu = new FormatDropdownMenu(formatDropdown, this.controllerIdentifier);
      }

      const fontFamilyDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="font-family"]');
      if (fontFamilyDropdown) {
         this.fontFamilyDropdownMenu = new FontFamilyDropdownMenu(fontFamilyDropdown, this.controllerIdentifier);
      }

      const fontSizeDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="font-size"]');
      if (fontSizeDropdown) {
         this.fontSizeDropdownMenu = new FontSizeDropdownMenu(fontSizeDropdown, this.controllerIdentifier);
      }

      const textAlignDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="text-align"]');
      if (textAlignDropdown) {
         this.textAlignDropdownMenu = new TextAlignDropdownMenu(textAlignDropdown, this.controllerIdentifier);
      }

      const lineHeightDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="line-height"]');
      if (lineHeightDropdown) {
         this.lineHeightDropdownMenu = new LineHeightDropdownMenu(lineHeightDropdown, this.controllerIdentifier);
      }

      const textColorDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="text-color"]');
      if (textColorDropdown) {
         this.textColorPalette = new TextColorDropdownMenu(textColorDropdown, this.controllerIdentifier, 'textCurrentColor');
      }

      const backgroundColorDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="background-color"]');
      if (backgroundColorDropdown) {
         this.backgroundColorPalette = new BackgroundColorDropdownMenu(backgroundColorDropdown, this.controllerIdentifier, 'backgroundCurrentColor');
      }
   }

   protected getComponents(): {
      dropdowns: { [key: string]: { path: string; component: UpdateableDropdownMenuBaseComponent<any>; }; };
      buttons?: { [key: string]: { selector: string; path: string; activeClass?: string; }; };
      text?: { [key: string]: { selector: string; path: string; }; };
   } {
      return {
         dropdowns: {
            fontFamily: {
               path: 'textStyle.fontFamily',
               component: this.fontFamilyDropdownMenu,
            },
            fontSize: {
               path: 'textStyle.fontSize',
               component: this.fontSizeDropdownMenu,
            },
            textAlign: {
               path: 'textAlign',
               component: this.textAlignDropdownMenu
            },
            formats: {
               path: 'formats',
               component: this.formatDropdownMenu,
            },
            textColor: {
               path: 'textStyle.color',
               component: this.textColorPalette,
            },
            highlight: {
               path: 'highlight',
               component: this.backgroundColorPalette,
            },
            lineHeight: {
               path: 'textStyle.lineHeight',
               component: this.lineHeightDropdownMenu,
            },
         },
         buttons: {
            invisibleCharacters: {
               selector: '[data-command="invisibleCharacters"]',
               path: 'invisibleCharacters',
               activeClass: 'icon-check',
            },
         }
      };
   }

   public destroy() {
      this.formatDropdownMenu.destroy();
      this.fontFamilyDropdownMenu.destroy();
      this.fontSizeDropdownMenu.destroy();
      this.textAlignDropdownMenu.destroy();
      this.lineHeightDropdownMenu.destroy();
      this.textColorPalette.destroy();
      this.backgroundColorPalette.destroy();
   }
}
