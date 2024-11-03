
import { IEditorState } from "wysiwyg/interfaces/IEditorState";
import { UpdateableDropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/UpdateableDropdownMenuBaseComponent";
import textAlignTemplate from "wysiwyg/view/components/dropdownMenus/templates/textAlign.html";

export class TextAlignDropdownMenu extends UpdateableDropdownMenuBaseComponent<IEditorState['textAlign']> {
   constructor(container: HTMLElement, controllerIdentifier: string, dropdownTextId?: string) {
      super(container, controllerIdentifier, 'text-align-param', dropdownTextId);
   }

   protected render(): HTMLElement {
      return this.createElementFromTemplate(textAlignTemplate);
   }

   protected getCheckPlace(newState: IEditorState['textAlign']): HTMLElement {
      return this.checkPlaces.get(newState ?? 'left')!;
   }

   public updateItems(newState: IEditorState['textAlign'], previousState: IEditorState['textAlign']): void {
      if (!newState) return;
      this.updateCheckIcon(newState, previousState);
   }

   public updateDropdownText(newState: IEditorState['textAlign']): void {
      const validAlignments = ['left', 'center', 'right', 'justify'];
      if (this.dropdownText && newState && validAlignments.includes(newState)) {
         this.dropdownText.classList.remove('icon-text-align-left', 'icon-text-align-center', 'icon-text-align-right', 'icon-text-align-justify');
         this.dropdownText.classList.add(`icon-text-align-${newState.toLowerCase()}`);
      }
   }
}