import { BaseComponent } from "wysiwyg/view/components/base/BaseComponent";
import { IEditorState } from "wysiwyg/interfaces/IEditorState";
import { UpdateableDropdownMenuBaseComponent } from "wysiwyg/view/components/dropdownMenus/base/UpdateableDropdownMenuBaseComponent";

export abstract class UpdateableComponent extends BaseComponent {
    protected abstract getComponents(): {
        dropdowns?: {
            [key: string]: {
                path: string;
                component: UpdateableDropdownMenuBaseComponent<any>;
            }
        },
        buttons?: {
            [key: string]: {
                selector: string;
                path: string;
                activeClass?: string;
            };
        };
        text?: {
            [key: string]: {
                selector: string;
                path: string;
            };
        };
    };

    public updateUI(newState: IEditorState, previousState: IEditorState | null): void {
        const config = this.getComponents();

        if (config.dropdowns) {
            for (const [key, { path, component }] of Object.entries(config.dropdowns)) {
                if (!component) {
                    console.error('component not found', key);
                    continue;
                };

                const currentValue = this.getStateValue(newState, path);
                const previousValue = previousState ? this.getStateValue(previousState, path) : null;

                if (this.shouldUpdateComponent(currentValue, previousValue)) {

                    component.updateItems(currentValue, previousValue);

                    if ('updateDropdownText' in component) {
                        component.updateDropdownText?.(currentValue);
                    }
                }
            }
        }

        if (config.buttons) {
            for (const [key, { selector, path, activeClass }] of Object.entries(config.buttons)) {
                const button = this.querySelector(selector);
                if (!button) continue;

                const currentValue = this.getStateValue(newState, path);
                const previousValue = previousState ? this.getStateValue(previousState, path) : null;

                if (this.shouldUpdateComponent(currentValue, previousValue)) {
                    this.toggleClass(button, activeClass || 'active', currentValue);
                }
            }
        }

        if (config.text) {
            for (const [key, { selector, path }] of Object.entries(config.text)) {
                const textElement = this.querySelector(selector);
                if (!textElement) continue;
                textElement.textContent = this.getStateValue(newState, path);
            }
        }
    }

    private getStateValue(state: any, path: string): any {
        return path.split('.').reduce((obj, key) => obj?.[key], state);
    }

    protected shouldUpdateComponent<T>(currentValue: T, previousValue: T | null): boolean {
        return !previousValue || JSON.stringify(currentValue) !== JSON.stringify(previousValue);
    }
}