
import { IEditorState } from "wysiwyg/interfaces/IEditorState";
import { IEditorView } from "wysiwyg/interfaces/IEditorView";
import { EditorContainer } from "wysiwyg/view/components/EditorContainer";
import { Menubar } from "wysiwyg/view/components/Menubar";
import { Toolbar } from "wysiwyg/view/components/Toolbar";
import { Footer } from "wysiwyg/view/components/Footer"
import { ImageModal } from "wysiwyg/view/components/modals/ImageModal"
import { LinkModal } from "wysiwyg/view/components/modals/LinkModal"
import { SourceCodeModal } from "wysiwyg/view/components/modals/SourceCodeModal"
import { SpecialCharactersModal } from "wysiwyg/view/components/modals/SpecialCharactersModal"
import { VideoModal } from "wysiwyg/view/components/modals/VideoModal"
import { Dropdown, Tooltip } from "bootstrap";
import { TableBubbleMenu } from "wysiwyg/view/components/bubbleMenus/TableBubbleMenu"
import { PreviewModal } from "wysiwyg/view/components/modals/PreviewModal";
import { SearchAndReplaceDialog } from "wysiwyg/view/components/dialogs/SearchAndReplaceDialog";

export class EditorView implements IEditorView {
   public readonly editorContainer: EditorContainer;
   public readonly menubar: Menubar;
   public readonly toolbar: Toolbar;
   public readonly footer: Footer;
   public readonly imageModal: ImageModal;
   public readonly linkModal: LinkModal;
   public readonly sourceCodeModal: SourceCodeModal;
   public readonly specialCharactersModal: SpecialCharactersModal;
   public readonly videoModal: VideoModal;
   public readonly previewModal: PreviewModal;
   public readonly searchAndReplaceDialog: SearchAndReplaceDialog;

   constructor(private container: HTMLElement, private controllerIdentifier: string) {
      this.editorContainer = new EditorContainer(container, controllerIdentifier);
      this.menubar = new Menubar(this.editorContainer.header, controllerIdentifier);
      this.toolbar = new Toolbar(this.editorContainer.header, controllerIdentifier);
      this.footer = new Footer(this.editorContainer.footer, controllerIdentifier);

      this.imageModal = new ImageModal(this.container, controllerIdentifier);
      this.linkModal = new LinkModal(this.container, controllerIdentifier);
      this.sourceCodeModal = new SourceCodeModal(this.container, controllerIdentifier);
      this.specialCharactersModal = new SpecialCharactersModal(this.container, controllerIdentifier);
      this.videoModal = new VideoModal(this.container, controllerIdentifier);
      this.previewModal = new PreviewModal(this.container, controllerIdentifier);

      const editorRect = this.editorContainer.body.getBoundingClientRect();
      this.searchAndReplaceDialog = new SearchAndReplaceDialog(this.container, controllerIdentifier, editorRect);

      this.initializeBootstrapDropdowns(this.container);
      this.initializeBootstrapTooltips(this.container);
   }

   private initializeBootstrapDropdowns(container: HTMLElement): void {
      const CLASS_NAME = 'has-child-dropdown-show';

      const originalToggle = Dropdown.prototype.toggle;
      Dropdown.prototype.toggle = function () {
          container.querySelectorAll('.' + CLASS_NAME).forEach(element => {
              element.classList.remove(CLASS_NAME);
          });
          let dd = this._element.closest('.dropdown').parentNode.closest('.dropdown');
          while (dd && dd !== container) {
              dd.classList.add(CLASS_NAME);
              dd = dd.parentNode.closest('.dropdown');
          }
          return originalToggle.call(this);
      };

      container.querySelectorAll('.dropdown').forEach(dropdown => {
          dropdown.addEventListener('hide.bs.dropdown', (event) => {
              if (dropdown.classList.contains(CLASS_NAME)) {
                  dropdown.classList.remove(CLASS_NAME);
                  event.preventDefault();
              }
              event.stopPropagation();
          });
      });
  }

  private initializeBootstrapTooltips(container: HTMLElement): void {
      container.querySelectorAll('[data-bs-toggle="tooltip"], [data-tooltip="tooltip"]').forEach((tooltip) => {
          new Tooltip(tooltip);
      });
  }

   public appendEditor(element: Element) {
      this.editorContainer.body.append(element);
   }

   public getTableBubbleMenuElement(): HTMLElement {
      return new TableBubbleMenu(this.container, this.controllerIdentifier).getBubbleMenu();
   }

   public toggleSearchAndReplace(): void {
      const editorRect = this.editorContainer.body.getBoundingClientRect();
      this.searchAndReplaceDialog.updatePosition(editorRect);
      this.searchAndReplaceDialog.toggleDialog();
   }

   public updateView(newState: IEditorState, previousState: IEditorState | null): void {
      if (!previousState || JSON.stringify(newState) !== JSON.stringify(previousState)) {
         this.menubar.updateUI(newState, previousState);
         this.toolbar.updateUI(newState, previousState);
         this.footer.updateUI(newState, previousState);
      }

      if (!previousState || JSON.stringify(previousState.searchResult) !== JSON.stringify(newState.searchResult)) {
         this.searchAndReplaceDialog.updateSearchResults(newState.searchResult);
     }
   }

   public destroy(): void {
      this.editorContainer.destroy();
      this.menubar.destroy();
      this.toolbar.destroy();
      this.footer.destroy();
      this.imageModal.destroy();
      this.linkModal.destroy();
      this.sourceCodeModal.destroy();
      this.specialCharactersModal.destroy();
      this.videoModal.destroy();
      this.searchAndReplaceDialog.destroy();
   }
}