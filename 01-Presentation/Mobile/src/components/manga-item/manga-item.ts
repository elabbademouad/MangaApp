import { Component, Input, OnInit } from '@angular/core';
import { RessourcesProvider } from '../../providers/ressources/ressources'
import { NavController, ToastController } from 'ionic-angular';
import { MangaDetailsViewModel } from '../../ViewModel/manga-details-View-model';
import { AppStorageProvider } from '../../providers/app-storage/app-storage';
import { ChooseChaptersPage } from '../../pages/choose-chapters-page/choose-chapters-page';
@Component({
  selector: 'manga-item',
  templateUrl: 'manga-item.html'
})

export class MangaItemComponent implements OnInit {
  /****************************************************
   * Constructor
   ****************************************************/
  constructor(public _ressources: RessourcesProvider,
    public navCtrl: NavController,
    public _toastCtrl: ToastController,
    public _appStorage: AppStorageProvider) {
    this.init();
  }
  /****************************************************
   * Public properties
  *****************************************************/
  @Input() item: MangaDetailsViewModel;
  ressources: any;
  /***************************************************
  * Initialize component
  ****************************************************/
  init() {
    this.ressources = this._ressources.stringResources;
  }
  ngOnInit() {
    this._appStorage.setFavorieOrDownlodedManga(this.item);
  }
  /***************************************************
   * UI event handler 
   ****************************************************/
  handleClickFavorie() {
    this.item.isFavorite = !this.item.isFavorite;
    this._appStorage.updateManga(this.item).then((data: any) => {
      this.showMessage();
    });
  }
  showMessage() {
    let toast = this._toastCtrl.create({
      message: (this.item.isFavorite) ? this.item.item.name + ' ' + this._ressources.stringResources.addFavoriteSuccess : this.item.item.name + ' ' + this._ressources.stringResources.removeFavorite,
      duration: 3000,
      cssClass: "toast"
    });
    toast.present();
  }
  handleDownloadClick() {
    this.navCtrl.push(ChooseChaptersPage, this.item);
  }
}