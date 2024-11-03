import { IEditorState } from "wysiwyg/interfaces/IEditorState";
import { BaseComponent } from "wysiwyg/view/components/base/BaseComponent";
import searchAndReplaceDialogTemplate from "wysiwyg/view/components/dialogs/templates/searchAndReplace.html";

export class SearchAndReplaceDialog extends BaseComponent {
    private isVisible: boolean = false;
    private searchResultIndexElement: HTMLElement;
    private searchResultTotalElement: HTMLElement;

    constructor(container: HTMLElement, controllerIdentifier: string, private rect: DOMRect) {
        super(container, controllerIdentifier);
        this.element.hidden = true;
    }

    protected render(): HTMLElement {
        return this.createElementFromTemplate(searchAndReplaceDialogTemplate);
    }

    protected initialize(): void {
        this.searchResultIndexElement = this.element.querySelector(`#searchResultIndex`) as HTMLElement;
        this.searchResultTotalElement = this.element.querySelector(`#searchResultTotal`) as HTMLElement;
        window.addEventListener('resize', () => this.updatePosition(this.rect));
    }

    public toggleDialog(): void {
        this.isVisible = !this.isVisible;
        this.element.hidden = !this.isVisible;

        if (this.isVisible) {
            this.setPosition();
        }
    }

    public updatePosition(rect: DOMRect): void {
        this.rect = rect;
        if (this.isVisible) {
            this.setPosition();
        }
    }

    private setPosition(): void {
        this.setStyle('top', `${this.rect.top + 50}px`);
        this.setStyle('right', `10px`);
    }

    public updateSearchResults(state: IEditorState['searchResult']): void {
        if (state) {
            this.searchResultIndexElement.textContent = state.index.toString();
            this.searchResultTotalElement.textContent = state.total.toString();
        }
    }

    public destroy(): void {
        window.removeEventListener('resize', () => this.updatePosition(this.rect));
        super.destroy();
    }
}