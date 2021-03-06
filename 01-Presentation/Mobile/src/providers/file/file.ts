import { Injectable } from '@angular/core';
import { File, FileEntry, Entry } from '@ionic-native/file'
@Injectable()
export class FileProvider {

    constructor(public _file: File) {
    }
    saveImageFile(fileName: string, base64: string, _callBack: any) {
        this._file.writeFile(this._file.dataDirectory, fileName, base64, { replace: true })
            .then((file: FileEntry) => {
                _callBack(file.toURL());
            });
    }

    getImageAsBase64(fileName: string): Promise<string> {
        return this._file.readAsText(this._file.dataDirectory, fileName)
    }
    
    removeFile(fileName:string){
        this._file.removeFile(this._file.dataDirectory,fileName);
    }

    getStorageFileSize(){
        this._file.listDir(this._file.dataDirectory,"").then((entry:Entry[])=>{
            entry.forEach(e=>{
                console.log(e.name);
                this._file.removeFile(this._file.dataDirectory,e.name);
            })
        });
    }
}
