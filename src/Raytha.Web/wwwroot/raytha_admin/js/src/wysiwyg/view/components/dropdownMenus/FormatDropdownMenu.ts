import { IEditorState } from "wysiwyg/interfaces/IEditorState";
import { UpdateableDropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/UpdateableDropdownMenuBaseComponent";
import formatTemplate from "wysiwyg/view/components/dropdownMenus/templates/format.html";

export class FormatDropdownMenu extends UpdateableDropdownMenuBaseComponent<IEditorState['formats']> {
   constructor(container: HTMLElement, controllerIdentifier: string, dropdownTextId?: string) {
      super(container, controllerIdentifier, 'data-item-role', dropdownTextId);
   }

   protected render(): HTMLElement {
      return this.createElementFromTemplate(formatTemplate);
   }

   protected getCheckPlace(newState: IEditorState['formats']): HTMLElement {
      const activeFormat = Object.entries(newState).find(([_, isActive]) => isActive);
      if (!activeFormat) return this.checkPlaces.get('paragraph')!;

      const [formatName] = activeFormat;
      return this.checkPlaces.get(formatName)!;
   }

   public updateItems(newState: IEditorState['formats'], previousState: IEditorState['formats']): void {
      if (!newState) return;

      this.checkPlaces.forEach((checkPlace, format) => {
         const isActive = newState[format as keyof IEditorState['formats']] === true;
         this.toggleClass(checkPlace, 'icon-check', isActive);
         this.toggleClass(checkPlace, 'icon-empty', !isActive);
      });
   }

   public updateDropdownText(newState: IEditorState['formats']): void {
      if (!this.dropdownText || !newState) return;

      const activeFormat = Object.entries(newState).find(([_, isActive]) => isActive);
      if (activeFormat) {
         const [formatName] = activeFormat;
         const displayName = this.formatNameToDisplayName(formatName);
         this.dropdownText.textContent = displayName;
      }
   }

   private formatNameToDisplayName(formatName: string): string {
      const formatDisplayNames: Record<string, string> = {
         'paragraph': 'Paragraph',
         'heading1': 'Heading 1',
         'heading2': 'Heading 2',
         'heading3': 'Heading 3',
         'heading4': 'Heading 4',
         'heading5': 'Heading 5',
         'heading6': 'Heading 6'
      };

      return formatDisplayNames[formatName] || formatName;
   }
}