import { DropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/DropdownMenuBaseComponent";
import table from "wysiwyg/view/components/dropdownMenus/templates/table.html";

export class TableDropdownMenu extends DropdownMenuBaseComponent {

   constructor(container: HTMLElement, controllerIdentifier: string) {
      super(container, controllerIdentifier);
   }

   protected render(): HTMLElement {
      return this.createElementFromTemplate(table);
   }

   protected initialize(): void {
      const tableSizeTextElement: HTMLElement = this.querySelector('.text-center')!;
      const cells = this.querySelectorAll('.table-cell');
      cells.forEach(cell => {
         this.bindEvent(cell, 'mouseover', (event) => {
            const currentCell = event.target as HTMLElement;
            const currentRow = parseInt(currentCell.dataset.row!);
            const currentCol = parseInt(currentCell.dataset.col!);

            this.updateTableSizeText(currentRow, currentCol, tableSizeTextElement);

            cells.forEach(targetCell => {
               const targetRow = parseInt(targetCell.dataset.row!);
               const targetCol = parseInt(targetCell.dataset.col!);

               if (targetRow <= currentRow && targetCol <= currentCol) {
                  targetCell.classList.add('active');
               } else {
                  targetCell.classList.remove('active');
               }
            });
         });
      });
   }

   private updateTableSizeText(row: number, col: number, tableSizeTextElement: HTMLElement): void {
      tableSizeTextElement.textContent = `${row} x ${col} table`;
   }
}