import { ModalBaseComponent } from "wysiwyg/view/components/modals/base/ModalBaseComponent";
import previewModalTemplate from "wysiwyg/view/components/modals/templates/previewModal.html";

export class PreviewModal extends ModalBaseComponent {
    protected render(): HTMLElement {
        return this.createElementFromTemplate(previewModalTemplate);
    }

    protected initialize(): void { }

    public setContent(html: string): void {
        const previewContainer = this.element.querySelector('.preview-content');
        if (previewContainer) {
            previewContainer.innerHTML = html;
        }
    }
}