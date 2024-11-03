import { IEditorState } from "wysiwyg/interfaces/IEditorState";
import { UpdateableDropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/UpdateableDropdownMenuBaseComponent";
import fontSizeTemplate from "wysiwyg/view/components/dropdownMenus/templates/fontSize.html";

export class FontSizeDropdownMenu extends UpdateableDropdownMenuBaseComponent<IEditorState['textStyle']['fontSize']> {
   constructor(container: HTMLElement, controllerIdentifier: string, dropdownTextId?: string) {
      super(container, controllerIdentifier, 'font-size-param', dropdownTextId);
   }

   protected render(): HTMLElement {
      return this.createElementFromTemplate(fontSizeTemplate);
   }

   protected getCheckPlace(newState: IEditorState['textStyle']['fontSize']): HTMLElement {
      return this.checkPlaces.get(newState ?? '16px') as HTMLElement;
   }

   public updateItems(newState: IEditorState['textStyle']['fontSize'], previousState: IEditorState['textStyle']['fontSize']): void {
      if (!newState) return;
      this.updateCheckIcon(newState, previousState);
   }

   public updateDropdownText(newState: IEditorState['textStyle']['fontSize']): void {
      if (this.dropdownText && newState) {
         this.dropdownText.textContent = newState;
      }
   }
}