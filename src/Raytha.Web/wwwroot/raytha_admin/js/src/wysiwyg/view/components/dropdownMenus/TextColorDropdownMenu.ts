import { IEditorState } from "wysiwyg/interfaces/IEditorState";
import { UpdateableDropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/UpdateableDropdownMenuBaseComponent";
import colorPaletteTemplate from "wysiwyg/view/components/dropdownMenus/templates/colorPalette.html";

export class TextColorDropdownMenu extends UpdateableDropdownMenuBaseComponent<IEditorState['textStyle']['color']> {
   constructor(container: HTMLElement, controllerIdentifier: string, dropdownTextId?: string) {
      super(container, controllerIdentifier, 'color-param', dropdownTextId);
   }

   protected render(): HTMLElement {
      const template = colorPaletteTemplate.replaceAll('{{command}}', 'setTextColor').trim();
      return this.createElementFromTemplate(template);
   }

   protected getCheckPlace(newState: IEditorState['textStyle']['color']): HTMLElement {
      return this.checkPlaces.get(newState ?? '#000000') as HTMLElement;
   }

   public updateItems(newState: IEditorState['textStyle']['color'], previousState: IEditorState['textStyle']['color']): void {
      if (!newState) return;
      this.updateCheckIcon(newState, previousState);
   }

   public updateDropdownText(newState: IEditorState['textStyle']['color']): void {
      if (this.dropdownText && newState) {
         this.dropdownText.setAttribute('fill', newState);
      }
   }
}