import { Controller } from "@hotwired/stimulus";
import { EditorView } from "../../wysiwyg/view/EditorView";
import { EditorModel } from "../../wysiwyg/model/EditorModel";

import Uppy from "@uppy/core";
import AwsS3 from "@uppy/aws-s3";
import XHRUpload from "@uppy/xhr-upload";
import Dashboard from "@uppy/dashboard";
import Swal from "sweetalert2";
import ImageEditor from "@uppy/image-editor";

import "@uppy/core/dist/style.min.css";
import "@uppy/dashboard/dist/style.min.css";
import "@uppy/image-editor/dist/style.min.css";

export default class extends Controller {
   static targets = ["editorContainer", "uppyDashboardContainer", "editorContent"];
   static values = {
      usedirectuploadtocloud: Boolean,
      mimetypes: String,
      maxfilesize: Number,
      pathbase: String,
   };

   connect() {
      this.editorView = new EditorView(this.editorContainerTarget, this.identifier);
      this.editorModel = new EditorModel(
         this.editorView.getTableBubbleMenuElement(),
         this.editorView.getListBubbleMenuElement()
      );

      this.editorView.appendEditor(this.editorModel.editor.options.element);

      this.editorView.linkModal.bindShowModal((event) => {
         const { from, to } = this.editorModel.editor.view.state.selection;
         const text = this.editorModel.editor.view.state.doc.textBetween(
            from,
            to,
            " "
         );
         const form = event.target.querySelector("form");
         form.elements.linkText.value = text;
      });

      this.editorView.sourceCodeModal.bindShowModal(() => {
         this.editorView.sourceCodeModal.getTextArea().value =
            this.editorModel.editor.getHTML();
      });

      this.editorView.previewModal.bindShowModal(() => {
         const content = this.editorModel.editor.getHTML();
         this.editorView.previewModal.setContent(content);
      });

      this.editorView.imageModal.bindHideModal(() => {
         this.uppy.cancelAll();
      });

      this.unsubscribe = this.editorModel.stateManager.subscribe(
         (state, previousState) => this.editorView.updateView(state, previousState)
      );

      this.initializeUppyDashboard();

      const initialContent = this.editorContentTarget.value;
      if (initialContent) {
         this.editorModel.commands.setContent(initialContent);
      }

      this.editorModel.stateManager.setContentChangeCallback((content) => {
         this.editorContentTarget.value = content;
      });
   }

   initializeUppyDashboard() {
      this.uppy = new Uppy({
         id: this.uppyDashboardContainerTarget,
         restrictions: {
            maxFileSize: this.maxfilesizeValue,
            maxNumberOfFiles: 1,
            allowedFileTypes: this.mimetypesValue.split(","),
         },
         autoProceed: false,
         allowMultipleUploads: false,
      });

      this.uppy.use(Dashboard, {
         inline: true,
         target: this.uppyDashboardContainerTarget,
         metaFields: [
            {
               id: "imageAltText",
               name: "Alternative description",
            },
         ],
      });

      this.uppy.use(ImageEditor);

      if (!this.usedirectuploadtocloud) {
         this.uppy.use(XHRUpload, {
            endpoint: `${this.pathbaseValue}/raytha/media-items/upload`,
         });
         this.uppy.on("upload-success", (file, response) => {
            const URL = `${this.pathbaseValue}/raytha/media-items/objectkey/${response.body.fields.objectKey}`;
            this.editorModel.commands.insertImageByLink(
               URL,
               file.meta.imageAltText
            );
         });
      } else {
         this.uppy.use(AwsS3, {
            getUploadParameters: (file) => {
               const URL = `${this.pathbaseValue}/raytha/media-items/presign`;
               return fetch(URL, {
                  method: "POST",
                  headers: {
                     Accept: "application/json",
                     "Content-Type": "application/json",
                  },
                  body: JSON.stringify({
                     filename: file.name,
                     contentType: file.type,
                     extension: file.extension,
                  }),
               })
                  .then((response) => response.json())
                  .then((data) => {
                     this.uppy.setFileMeta(file.id, {
                        id: data.fields.id,
                        objectKey: data.fields.objectKey,
                     });
                     return {
                        method: "PUT",
                        url: data.url,
                        fields: data.fields,
                        headers: {
                           "x-ms-blob-type": "BlockBlob",
                        },
                     };
                  });
            },
         });
         this.uppy.on("upload-success", (file, response) => {
            const URL = `${this.pathbaseValue}/raytha/media-items/objectkey/${file.meta.objectKey}`;
            this.editorModel.commands.insertImageByLink(
               URL,
               file.meta.imageAltText
            );

            //make post call
            const CREATE_MEDIA_ENDPOINT = `${this.pathbaseValue}/raytha/media-items/create-after-upload`;
            fetch(CREATE_MEDIA_ENDPOINT, {
               method: "POST",
               headers: {
                  Accept: "application/json",
                  "Content-Type": "application/json",
               },
               body: JSON.stringify({
                  filename: file.name,
                  contentType: file.type,
                  extension: file.extension,
                  id: file.meta.id,
                  objectKey: file.meta.objectKey,
                  length: file.size,
               }),
            });
         });
      }

      this.uppy.on("restriction-failed", (file, error) => {
         Swal.fire({
            title: "File Restriction",
            text: error,
            showConfirmButton: false,
            showCloseButton: true,
            showCancelButton: true,
            cancelButtonText: "OK",
            icon: "error",
         });
      });

      this.uppy.on("upload-error", (file, error, response) => {
         console.log("error with file:", file.id);
         console.log("error message:", error);
         Swal.fire({
            title: "Upload failed",
            text: error,
            showConfirmButton: false,
            showCloseButton: true,
            showCancelButton: true,
            cancelButtonText: "OK",
            icon: "error",
         });
      });

      this.uppy.on("upload-success", (file, response) => {
         console.log("upload-success", response);
         const imageUrl = response.uploadURL;
         const imageAltText = file.meta.imageAltText;

         this.editorModel.commands.insertImage(imageUrl, { alt: imageAltText });
      });

      this.uppy.on("complete", () => {
         this.editorView.imageModal.hide();
      });
   }

   executeCommand(event) {
      const command = event.params.command;
      if (!this.isValidCommand(command)) {
         console.error(`Invalid command: ${command}`);

         return;
      }

      try {
         const result = this.editorModel.commands[command]();
         if (result !== false && result !== undefined) {
            this.editorModel.stateManager.forceUpdate();
         }
      } catch (error) {
         console.error(error);
      }
   }

   setFontFamily({ params: { fontFamily } }) {
      this.editorModel.commands.setFontFamily(fontFamily);
      this.editorModel.stateManager.forceUpdate();
   }

   setFontSize({ params: { fontSize } }) {
      this.editorModel.commands.setFontSize(fontSize);
      this.editorModel.stateManager.forceUpdate();
   }

   insertHeading({ params: { level } }) {
      this.editorModel.commands.insertHeading(level);
      this.editorModel.stateManager.forceUpdate();
   }

   setTextAlign({ params: { textAlign } }) {
      this.editorModel.commands.setTextAlign(textAlign);
   }

   setLineHeight({ params: { lineHeight } }) {
      this.editorModel.commands.setLineHeight(lineHeight);
      this.editorModel.stateManager.forceUpdate();
   }

   insertTable(event) {
      const rows = parseInt(event.target.dataset.row);
      const cols = parseInt(event.target.dataset.col);

      this.editorModel.commands.insertTable(rows, cols);
      this.editorModel.stateManager.forceUpdate();
   }

   setTextColor({ params: { color } }) {
      this.editorModel.commands.setTextColor(color);
      this.editorModel.stateManager.forceUpdate();
   }

   setBackgroundColor({ params: { color } }) {
      this.editorModel.commands.setBackgroundColor(color);
      this.editorModel.stateManager.forceUpdate();
   }

   insertLink(event) {
      event.preventDefault();
      const {
         linkUrl: { value: url },
         linkText: { value: text },
         linkTitle: { value: title },
         linkOpenInNewWindow: { checked: openInNewWindow },
      } = event.target.elements;

      if (url && text) {
         this.editorModel.commands.insertLink(
            url,
            text,
            title,
            openInNewWindow
         );

         this.editorView.linkModal.hide();
      }
   }

   insertImageByUrl(event) {
      event.preventDefault();

      const {
         imageUrl: { value: imageUrl },
         imageAltText: { value: imageAltText },
      } = event.target.elements;

      this.editorModel.commands.insertImageByLink(imageUrl, imageAltText);

      this.editorView.imageModal.hide();
   }

   insertVideo(event) {
      event.preventDefault();

      const {
         videoUrl: { value: videoUrl },
         videoWidth: { value: videoWidth },
         videoHeight: { value: videoHeight },
      } = event.target.elements;

      const result = this.editorModel.commands.insertYoutubeVideo(
         videoUrl,
         Number(videoWidth),
         Number(videoHeight)
      );

      if (result) {
         this.editorView.videoModal.hide();
      } else {
         Swal.fire({
            title: "Invalid URL",
            text: "Please enter a valid YouTube URL",
            icon: "error",
            showConfirmButton: false,
            showCloseButton: true,
            showCancelButton: true,
            cancelButtonText: "OK",
         });
      }
   }

   updateSourceCode(event) {
      event.preventDefault();

      const {
         sourceCodeTextarea: { value: sourceCode },
      } = event.target.elements;

      try {
         this.editorModel.commands.setContent(sourceCode);
         this.editorView.sourceCodeModal.hide();
      } catch (error) {
         Swal.fire({
            title: "Invalid content",
            text: error.toString(),
            icon: "error",
            showConfirmButton: false,
            showCloseButton: true,
            showCancelButton: true,
            cancelButtonText: "OK",
         });
      }
   }

   insertSpecialCharacter({ params: { character } }) {
      this.editorModel.commands.insertSpecialCharacter(character);
      this.editorView.specialCharactersModal.hide();
   }

   insertDateTime(event) {
      this.editorModel.commands.insertDateTime(event.target.textContent);
   }

   toggleSearchAndReplaceDialog() {
      this.editorView.toggleSearchAndReplace();
   }

   setSearchTerm(event) {
      const searchTerm = event.srcElement.value;
      this.editorModel.commands.setSearchTerm(searchTerm);
      this.editorModel.stateManager.forceUpdate();
   }

   setReplaceTerm(event) {
      const replaceTerm = event.srcElement.value;
      this.editorModel.commands.setReplaceTerm(replaceTerm);
      this.editorModel.stateManager.forceUpdate();
   }

   setCaseSensitive(event) {
      const isCaseSensitive = event.srcElement.checked;
      this.editorModel.commands.setCaseSensitive(isCaseSensitive);
      this.editorModel.stateManager.forceUpdate();
   }

   isValidCommand(command) {
      return (
         typeof command === "string" &&
         command in this.editorModel.commands &&
         typeof this.editorModel.commands[command] === "function"
      );
   }

   disconnect() {
      if (this.unsubscribe) {
         this.unsubscribe();
      }
      this.editorModel.destroy();
      this.editorView.destroy();
      this.uppy.destroy();
   }
}
