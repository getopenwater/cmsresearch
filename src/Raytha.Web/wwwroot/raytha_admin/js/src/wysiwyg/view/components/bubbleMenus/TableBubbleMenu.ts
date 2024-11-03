import { BubbleMenuBaseComponent } from "wysiwyg/view/components/bubbleMenus/base/BubbleMenuBaseComponent";
import tableBubbleMenuTemplate from "wysiwyg/view/components/bubbleMenus/templates/tableBubbleMenu.html";

export class TableBubbleMenu extends BubbleMenuBaseComponent {
   protected render(): HTMLElement {
      return this.createElementFromTemplate(tableBubbleMenuTemplate);
   }
}