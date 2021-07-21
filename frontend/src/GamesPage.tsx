import { useEffect, useState, FC } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import './GamesPage.css';
import { GameList } from './GameList';
import { Page } from './Page';
import { PageTitle } from './PageTitle';
import { PageSelect } from './PageSelect';
import { getGameList, GameSummary } from './GameListData';

interface RouteParams {
  userName: string;
  page?: string;
}

export const GamesPage: FC<RouteComponentProps<RouteParams>> =
({
  match
}) => {
  const [gamelist, setGameList]
    = useState<GameSummary | null>(null);
  const [gameListLoading, setGameListLoading] = useState(true);
  useEffect(() => {
    const doGetGameList = async () => {
      if (gameListLoading)
      {
        const glist = await getGameList(match.params.userName, parseInt(match.params.page ?? "0"));
        setGameList(glist);
        setGameListLoading(false);
      }
    };
    doGetGameList();
  });
  console.log("Render");
  return (
    <Page>
      <div className="GamesPageHeader">
        <PageTitle>Games</PageTitle>
      </div>
      {gamelist == null ? (
        <div>
        Loading...
        </div>
      ) : (
        <div>
        <PageSelect userName={`${match.params.userName}`} currentPage={gamelist.currentPage} totalPages={gamelist.totalPages} setGameListLoading={() => { setGameListLoading(true); } } />
        <GameList userName={`${match.params.userName}`} data={gamelist.games || []} />
        <PageSelect userName={`${match.params.userName}`} currentPage={gamelist.currentPage} totalPages={gamelist.totalPages} setGameListLoading={() => { setGameListLoading(true); } } />
        </div>
      )}
    </Page>
  );
};
