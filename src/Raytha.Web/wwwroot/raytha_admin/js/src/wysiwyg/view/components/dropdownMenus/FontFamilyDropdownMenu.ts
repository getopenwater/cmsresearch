import { IEditorState } from "wysiwyg/interfaces/IEditorState";
import { UpdateableDropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/UpdateableDropdownMenuBaseComponent";
import fontFamilyDropdownMenuTemplate from "wysiwyg/view/components/dropdownMenus/templates/fontFamily.html";

export class FontFamilyDropdownMenu extends UpdateableDropdownMenuBaseComponent<IEditorState['textStyle']['fontFamily']> {
   constructor(container: HTMLElement, controllerIdentifier: string, dropdownTextId?: string) {
      super(container, controllerIdentifier, 'font-family-param', dropdownTextId);
   }

   protected render(): HTMLElement {
      return this.createElementFromTemplate(fontFamilyDropdownMenuTemplate);
   }

   protected getCheckPlace(newState: IEditorState['textStyle']['fontFamily']): HTMLElement {
      return this.checkPlaces.get(newState ?? 'Helvetica') as HTMLElement;
   }

   public updateItems(newState: IEditorState['textStyle']['fontFamily'], previousState: IEditorState['textStyle']['fontFamily']): void {
      if (!newState) return;
      this.updateCheckIcon(newState, previousState);
   }

   public updateDropdownText(newState: IEditorState['textStyle']['fontFamily']): void {
      if (this.dropdownText && newState) {
         this.dropdownText.textContent = newState;
      }
   }
}