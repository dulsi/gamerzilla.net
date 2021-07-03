import { useEffect, useState, FC } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import './GamesPage.css';
import { GameList } from './GameList';
import { Page } from './Page';
import { PageTitle } from './PageTitle';
import { getGameList, GameListData } from './GameListData';

interface RouteParams {
  userName: string;
}

export const GamesPage: FC<RouteComponentProps<RouteParams>> =
({
  match
}) => {
  const [gamelist, setGameList]
    = useState<GameListData[] | null>(null);
  const [gameListLoading, setGameListLoading] = useState(true);
  useEffect(() => {
    const doGetGameList = async () => {
      if (gameListLoading)
      {
        const glist = await getGameList(match.params.userName);
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
        <GameList userName={`${match.params.userName}`} data={gamelist || []} />
      )}
    </Page>
  );
};
