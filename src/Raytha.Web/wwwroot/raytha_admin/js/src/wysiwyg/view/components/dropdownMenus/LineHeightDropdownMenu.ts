import { IEditorState } from "wysiwyg/interfaces/IEditorState";
import { UpdateableDropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/UpdateableDropdownMenuBaseComponent";
import lineHeightDropdownMenuTemplate from "wysiwyg/view/components/dropdownMenus/templates/lineHeight.html";

export class LineHeightDropdownMenu extends UpdateableDropdownMenuBaseComponent<IEditorState['textStyle']['lineHeight']> {
   constructor(container: HTMLElement, controllerIdentifier: string, dropdownTextId?: string) {
      super(container, controllerIdentifier, 'line-height-param', dropdownTextId);
   }

   protected render(): HTMLElement {
      return this.createElementFromTemplate(lineHeightDropdownMenuTemplate);
   }

   protected getCheckPlace(newState: IEditorState['textStyle']['lineHeight']): HTMLElement {
      return this.checkPlaces.get(newState ?? '1.4') as HTMLElement;
   }

   public updateItems(newState: IEditorState['textStyle']['lineHeight'], previousState: IEditorState['textStyle']['lineHeight']): void {
      if (!newState) return;
      this.updateCheckIcon(newState, previousState);
   }

   public updateDropdownText(newState: IEditorState['textStyle']['lineHeight']): void {
      if (this.dropdownText && newState) {
         this.dropdownText.textContent = newState;
      }
   }
}