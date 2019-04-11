import { Component} from '@angular/core';
import { NavController,  NavParams} from 'ionic-angular';
import { RessourcesProvider} from '../../providers/ressources/ressources'
import { LoadingController} from 'ionic-angular'
import { ChapterViewModel} from '../../ViewModel/chapter-view-model';
import { PageController} from '../../providers/controllers/page-controller';
import { Page} from '../../Model/page-model';
import { ChapterController } from '../../providers/controllers/chapter-Controller';
@Component({
  selector: 'manga-page',
  templateUrl: 'manga-page.html'
})
export class MangaPagePage {

  /****************************************************
   * Constructor
   ****************************************************/
  constructor(public navCtrl: NavController,
    public navParams: NavParams,
    public _pageCtr: PageController,
    public _ressources: RessourcesProvider,
    public _loading: LoadingController,
    public _chapterCtr: ChapterController) {
    this.int();
  }
  /***************************************************
   * Initialize component
   ****************************************************/
  int() {
    this.chapterVm = this.navParams.data;
    this.ressources = this._ressources.stringResources;
    let loading = this._loading.create({
      content: this.ressources.loading
    });
    loading.present();
    this._pageCtr.getByChapterId(this.chapterVm.chapter.id)
      .subscribe((data) => {
        this.pages = data;
        loading.dismiss();
        this._chapterCtr.getNextChapter(this.chapterVm.chapter.mangaId,Number(this.chapterVm.chapter.number))
          .subscribe(data=>{
            if(data !==null && data !== undefined){
              this.nextChapterVm=new ChapterViewModel();
              this.nextChapterVm.chapter=data;
            }
        });
        this._chapterCtr.getPreviousChapter(this.chapterVm.chapter.mangaId,Number(this.chapterVm.chapter.number))
          .subscribe(data=>{
            if(data !==null && data !== undefined){
              this.previousChapterVm=new ChapterViewModel();
              this.previousChapterVm.chapter=data;
            }
        })
      })
  }

  handleClickNextChapter(){
    this.navCtrl.push(MangaPagePage, this.nextChapterVm);
  }
  handleClickPreviousChapter(){
    this.navCtrl.push(MangaPagePage, this.previousChapterVm);
  }
  /****************************************************
   * Public properties
   *****************************************************/
  ressources: any;
  chapterVm: ChapterViewModel;
  nextChapterVm: ChapterViewModel;
  previousChapterVm: ChapterViewModel;
  pages: Array < Page >
}
