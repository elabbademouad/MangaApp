import { BrowserModule } from '@angular/platform-browser';
import { ErrorHandler, NgModule } from '@angular/core';
import { IonicApp, IonicErrorHandler, IonicModule } from 'ionic-angular';
import { MyApp } from './app.component';
import { HomePage } from '../pages/home/home';
import { ListPage } from '../pages/list/list';
import { MangaDetailsPage } from '../pages/manga-details/manga-details'
import { HttpClientModule } from '@angular/common/http'; 
import { StatusBar } from '@ionic-native/status-bar';
import { SplashScreen } from '@ionic-native/splash-screen';
import { RessourcesProvider } from '../providers/ressources/ressources';
import { MangaPagePage} from '../pages/manga-page/manga-page';
import { MangaFavorisPage} from '../pages/manga-favoris/manga-favoris';
import { SQLite} from '@ionic-native/sqlite'
import { MangaDownloadsPage} from '../pages/manga-downloads/manga-downloads'
import { MangaItemComponent } from '../components/manga-item/manga-item';
import { MangaController } from '../providers/controllers/manga-Controller';
import { ChapterController } from '../providers/controllers/chapter-Controller';
import { PageController } from '../providers/controllers/page-controller';
import { TagController } from '../providers/controllers/tag-controller';
import { IonicImageViewerModule } from 'ionic-img-viewer';
import { RecentsPage } from '../pages/recents/recent-page'
import { MangaSectionComponent } from '../components/manga-section/manga-section';
import { RatingStatusComponent } from '../components/rating-status/rating-status';
import { AppStorageProvider } from '../providers/app-storage/app-storage';
import { IonicStorageModule} from '@ionic/storage'
@NgModule({
  declarations: [
    MyApp,
    HomePage,
    ListPage,
    MangaDetailsPage,
    MangaPagePage,
    MangaDownloadsPage,
    MangaFavorisPage,
    MangaItemComponent,
    RecentsPage,
    MangaSectionComponent,
    RatingStatusComponent
  ],
  imports: [
    BrowserModule,
    IonicModule.forRoot(MyApp),
    HttpClientModule,
    IonicImageViewerModule,
    IonicStorageModule.forRoot(),
  ],
  bootstrap: [IonicApp],
  entryComponents: [
    MyApp,
    HomePage,
    ListPage,
    MangaDetailsPage,
    MangaPagePage,
    MangaDownloadsPage,
    MangaFavorisPage,
    MangaItemComponent,
    RecentsPage,
    MangaSectionComponent,
    RatingStatusComponent
  ],
  providers: [
    StatusBar,
    SplashScreen,
    {provide: ErrorHandler, useClass: IonicErrorHandler},
    RessourcesProvider,
    SQLite,
    MangaController,
    ChapterController,
    PageController,
    TagController,
    AppStorageProvider
  ]
})
export class AppModule {}
