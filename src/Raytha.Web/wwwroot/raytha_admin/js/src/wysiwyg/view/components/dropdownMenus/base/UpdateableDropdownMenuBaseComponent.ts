import { DropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/DropdownMenuBaseComponent";

export abstract class UpdateableDropdownMenuBaseComponent<T> extends DropdownMenuBaseComponent {
    protected checkPlaces: Map<string, HTMLElement>;
    protected dropdownButton: HTMLElement;
    protected dropdownText: HTMLElement | null;

    constructor(container: HTMLElement, controllerIdentifier: string, private itemAttribute: string, private dropdownTextId?: string) {
        super(container, controllerIdentifier);

        this.dropdownText = this.dropdownTextId
            ? this.container.querySelector(`#${this.dropdownTextId}`)
            : null;

        this.dropdownButton = this.container.querySelector('[data-bs-toggle="dropdown"]') as HTMLElement;

        this.initializeCheckPlaces();
    }

    protected initialize() {}

    private initializeCheckPlaces(): void {
        this.checkPlaces = new Map();
        Array.from(this.container.querySelectorAll('.dropdown-item')).forEach(item => {
            const key: string | undefined = item.getAttributeNames().find(name => name.includes(this.itemAttribute));
            const checkPlace: HTMLElement = (item.lastElementChild ?? item) as HTMLElement;
            const value: string | null = key ? item.getAttribute(key) : null;
            if (value) {
                this.checkPlaces.set(value, checkPlace);
            }
        });

        if (this.checkPlaces.size === 0) {
            console.error('checkPlaces not found', this.itemAttribute);
        }
    }

    protected updateCheckIcon(newState: T, previousState: T): void {
        if (previousState) {
            const oldCheckPlace = this.getCheckPlace(previousState);
            if (oldCheckPlace) {
                this.toggleClass(oldCheckPlace, 'icon-check', false);
                this.toggleClass(oldCheckPlace, 'icon-empty', true);
            }
        }

        const checkPlaces = this.getCheckPlace(newState);
        if (checkPlaces) {
            this.toggleClass(checkPlaces, 'icon-check', true);
            this.toggleClass(checkPlaces, 'icon-empty', false);
        }
    }

    protected abstract getCheckPlace(newState: T): HTMLElement | undefined;

    public abstract updateItems(newState: T, previousState: T): void;
    public abstract updateDropdownText?(newState: T): void;
}