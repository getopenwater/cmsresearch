import { UpdateableComponent } from "wysiwyg/view/components/base/UpdateableComponent";
import { UpdateableDropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/UpdateableDropdownMenuBaseComponent"
import { TableDropdownMenu } from "wysiwyg/view/components/dropdownMenus/TableDropdownMenu";
import { FormatDropdownMenu } from "wysiwyg/view/components/dropdownMenus/FormatDropdownMenu"
import { FontFamilyDropdownMenu } from "wysiwyg/view/components/dropdownMenus/FontFamilyDropdownMenu"
import { FontSizeDropdownMenu } from "wysiwyg/view/components/dropdownMenus/FontSizeDropdownMenu"
import { TextAlignDropdownMenu } from "wysiwyg/view/components/dropdownMenus/TextAlignDropdownMenu"
import toolbarTemplate from "wysiwyg/view/components/templates/toolbar.html";

export class Toolbar extends UpdateableComponent {
   private fontFamilyDropdownMenu: FontFamilyDropdownMenu;
   private fontSizeDropdownMenu: FontSizeDropdownMenu;
   private formatDropdownMenu: FormatDropdownMenu;
   private textAlignDropdownMenu: TextAlignDropdownMenu;

   protected render(): HTMLElement {
      return this.createElementFromTemplate(toolbarTemplate);
   }

   protected initialize(): void {
      const tableDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="table"]');
      if (tableDropdown) {
         new TableDropdownMenu(tableDropdown, this.controllerIdentifier);
      }

      const fontFamilyDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="fontFamily"]');
      if (fontFamilyDropdown) {
         this.fontFamilyDropdownMenu = new FontFamilyDropdownMenu(fontFamilyDropdown, this.controllerIdentifier, 'fontFamilySpan');
      }

      const fontSizeDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="font-size"]');
      if (fontSizeDropdown) {
         this.fontSizeDropdownMenu = new FontSizeDropdownMenu(fontSizeDropdown, this.controllerIdentifier, 'fontSizeSpan');
      }

      const formatDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="format"]');
      if (formatDropdown) {
         this.formatDropdownMenu = new FormatDropdownMenu(formatDropdown, this.controllerIdentifier, 'formatSpan');
      }

      const textAlignDropdown: HTMLElement | null = this.querySelector('[data-dropdown-role="text-align"]');
      if (textAlignDropdown) {
         this.textAlignDropdownMenu = new TextAlignDropdownMenu(textAlignDropdown, this.controllerIdentifier, 'textAlignButton');
      }

      const toolbarButtons = this.element.querySelector('#toolbarButtons') as HTMLElement;
      const collapseMenu = this.element.querySelector('#collapseMenu .btn-toolbar') as HTMLElement;
      const showMoreButtons = this.element.querySelector('#showMoreButtons') as HTMLElement;

      if (!toolbarButtons || !collapseMenu || !showMoreButtons) {
         return;
      }

      const toolbarItems = Array.from(
         toolbarButtons.querySelectorAll('.btn-group:not(#showMoreButtons), .dropdown:not(#showMoreButtons .dropdown)')
      ) as HTMLElement[];

      const toolbarWidth = toolbarButtons.offsetWidth;
      let currentWidth = showMoreButtons.offsetWidth;

      toolbarItems.forEach(item => {
         currentWidth += item.offsetWidth + this.getMarginRight(item);

         if (currentWidth > toolbarWidth) {
            const parentGroup = item.closest('.btn-group');
            if (parentGroup && parentGroup !== item) {
               collapseMenu.appendChild(parentGroup);
            } else {
               collapseMenu.appendChild(item);
            }
         }
      });
   }

   //todo resize toolbar
   private getMarginRight(element: HTMLElement): number {
      const marginRight = window.getComputedStyle(element).marginRight;
      return parseInt(marginRight, 10) || 0;
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
               component: this.fontFamilyDropdownMenu
            },
            fontSize: {
               path: 'textStyle.fontSize',
               component: this.fontSizeDropdownMenu,
            },
            textAlign: {
               path: 'textAlign',
               component: this.textAlignDropdownMenu,
            },
            formats: {
               path: 'formats',
               component: this.formatDropdownMenu,
            }
         },
         buttons: {
            bold: {
               selector: '#boldButton',
               path: 'marks.bold',
               activeClass: 'active',
            },
            italic: {
               selector: '#italicButton',
               path: 'marks.italic'
            },
            underline: {
               selector: '#underlineButton',
               path: 'marks.underline'
            },
            strike: {
               selector: '#strikeButton',
               path: 'marks.strike'
            },
            superscript: {
               selector: '#superscriptButton',
               path: 'marks.superscript'
            },
            subscript: {
               selector: '#subscriptButton',
               path: 'marks.subscript'
            },
            code: {
               selector: '#codeButton',
               path: 'marks.code'
            },
            bulletList: {
               selector: '#bulletListButton',
               path: 'list.bullet'
            },
            orderedList: {
               selector: '#orderedListButton',
               path: 'list.ordered'
            },
            blockquote: {
               selector: '#blockquoteButton',
               path: 'blockquote'
            },
            codeBlock: {
               selector: '#codeBlockButton',
               path: 'codeBlock'
            },
            invisibleCharacters: {
               selector: '[data-command="invisibleCharacters"]',
               path: 'invisibleCharacters',
            }
         }
      };
   }

   public destroy() {
      this.fontFamilyDropdownMenu.destroy();
      this.fontSizeDropdownMenu.destroy();
      this.formatDropdownMenu.destroy();
      this.textAlignDropdownMenu.destroy();
   }
}