import { Controller } from 'stimulus'
import Uppy from '@uppy/core'
import AwsS3 from '@uppy/aws-s3'
import XHRUpload from '@uppy/xhr-upload'
import Dashboard from '@uppy/dashboard';
import Swal from 'sweetalert2'
import ImageEditor from '@uppy/image-editor'

import '@uppy/core/dist/style.min.css'
import '@uppy/dashboard/dist/style.min.css'
import '@uppy/image-editor/dist/style.min.css';
import '@uppy/status-bar/dist/style.min.css';

export default class extends Controller {
   static values = {
      fieldid: String,
      pathbase: String,
      themeid: String,
   }

   static targets = ['uppyContainer', 'imageBase64', 'imageFileType', 'imageFileName']

   connect() {
      console.log("connected")
      this.uppy = new Uppy({
         id: this.fieldidValue,
         restrictions: {
            maxFileSize: 2000000,
            maxNumberOfFiles: 1,
            allowedFileTypes: ["image/*"],
         },
         autoProceed: false,
         allowMultipleUploads: false,
      })

      this.uppy.use(Dashboard, {
         inline: true,
         target: `#${this.fieldidValue}-uppy`,
         showProgressDetails: true,
         note: 'Images only, 1 file, up to 2 MB',
         singleFileFullScreen: true,
      })

      this.uppy.use(ImageEditor);

      this.uppy.on('restriction-failed', (file, error) => {
         Swal.fire({
            title: "File Restriction",
            text: error,
            showConfirmButton: false,
            showCloseButton: true,
            showCancelButton: true,
            cancelButtonText: "OK",
            icon: "error"
         });
      })

      if (this.themeidValue) {
         if (!this.usedirectuploadtocloudValue) {
            this.uppy.use(XHRUpload,
               {
                  endpoint: `${this.pathbaseValue}/raytha/media-items/upload?themeId=${this.themeidValue}&IsThemePreviewImage=true`
               })
         } else {
            this.uppy.use(AwsS3,
               {
                  getUploadParameters: file => {
                     const URL = `${this.pathbaseValue}/raytha/media-items/presign?themeId=${this.themeidValue}&IsThemePreviewImage=true`;
                     return fetch(URL,
                        {
                           method: 'POST',
                           headers: {
                              'Accept': 'application/json',
                              'Content-Type': 'application/json'
                           },
                           body: JSON.stringify({
                              filename: file.name,
                              contentType: file.type,
                              extension: file.extension
                           })
                        })
                        .then(response => response.json())
                        .then(data => {
                           return {
                              method: 'PUT',
                              url: data.url,
                              fields: data.fields,
                              headers: {
                                 'x-ms-blob-type': 'BlockBlob'
                              }
                           }
                        });
                  }
               })
            this.uppy.on('upload-success',
               (file, response) => {
                  console.log(response);
                  const CREATE_MEDIA_ENDPOINT = `${this.pathbaseValue}/raytha/media-items/create-after-upload?themeId=${this.themeidValue}`;

                  //make post call
                  fetch(CREATE_MEDIA_ENDPOINT,
                     {
                        method: 'POST',
                        headers: {
                           'Accept': 'application/json',
                           'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                           filename: file.name,
                           contentType: file.type,
                           extension: file.extension,
                           id: file.meta.id,
                           objectKey: file.meta.objectKey,
                           length: file.size
                        })
                     })
               })
         }
      }
      else {
         this.uppy.on('file-added', (file) => {
            console.log('Added file', file);
            this.convertFileToBase64(file.data)
               .then(base64 => {
                  const base64Data = base64.replace(/^data:image\/[a-zA-Z]+;base64,/, '');
                  this.imageBase64Target.value = base64Data;
                  this.imageFileTypeTarget.value = file.type;
                  this.imageFileNameTarget.value = file.name;

                  console.log('Base64:', base64);
                  console.log('Type:', file.type);
                  console.log('Name:', file.name);
               })
               .catch(error => {
                  console.error('Error converting file to base64:', error);
               });
         });

         this.uppy.on('file-removed', (file, reason) => {
            console.log('Removed file', file);
            this.imageBase64Target.value = '';
            this.imageFileTypeTarget.value = '';
            this.imageFileNameTarget.value = '';
         });

         this.uppy.on('upload-success', (file, response) => {
            console.log('File uploaded successfully:', file);
            this.convertFileToBase64(file.data)
               .then(base64 => {
                  this.imageBase64Target.value = base64;
                  this.imageFileTypeTarget.value = file.type;
                  this.imageFileNameTarget.value = file.name;
                  console.log('Base64:', base64);
                  console.log('Type:', file.type);
                  console.log('Name:', file.name);
               })
               .catch(error => {
                  console.error('Error converting file to base64:', error);
               });
         });
      }

   }

   convertFileToBase64(file) {
      return new Promise((resolve, reject) => {
         const reader = new FileReader();
         reader.readAsDataURL(file);
         reader.onload = () => resolve(reader.result);
         reader.onerror = error => reject(error);
      });
   }
}
