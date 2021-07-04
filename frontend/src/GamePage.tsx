import { useEffect, useState, FC, Fragment } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import { Page } from './Page';
import { PageTitle } from './PageTitle';
import { getGame, GameData } from './GameListData';
import { relativeAPIUrl } from './AppSettings';
import './GamePage.css';

interface RouteParams {
  userName: string;
  gameId: string;
}

export const GamePage: FC<RouteComponentProps<RouteParams>> =
({
  match
}) => {
  const [game, setGame]
    = useState<GameData | null>(null);
  const [gameLoading, setGameLoading] = useState(true);
  useEffect(() => {
    const doGetGame = async () => {
      if (gameLoading)
      {
        const glist = await getGame(match.params.userName, match.params.gameId);
        setGame(glist);
        setGameLoading(false);
      }
    };
    doGetGame();
  });
  return (
    <Page>
      {game ? (
        <div>
          <PageTitle><Fragment><img alt={`${game.name}`} src={`${relativeAPIUrl}/game/image/show?game=${game.shortname}`} />{game.name}</Fragment></PageTitle>
          <div>Achievements</div>
          <div className="TrophyList">
            <div className="TrophyListItem TrophyListRow">
              <div className="TrophyListColumn TrophyListStatus">Status</div>
              <div className="TrophyListColumn TrophyListTitle">Name</div>
              <div className="TrophyListColumn TrophyListProgress">Progress</div>
            </div>
            {game.trophy.map(trophy => (
              <div key={trophy.trophy_name} className="TrophyListItem TrophyListRow">
                <div className="TrophyListColumn TrophyListStatus"><img alt={`${trophy.achieved === "1"? "Achieved": "Not Achieved"}`} src={`${relativeAPIUrl}/trophy/image/show?game=${game.shortname}&trophy=${trophy.trophy_name}&achieved=${trophy.achieved}`} /></div>
                <div className="TrophyListColumn TrophyListTitle">{trophy.trophy_name}</div>
                <div className="TrophyListColumn TrophyListProgress">{trophy.progress}/{trophy.max_progress}</div>
                <div className="TrophyListColumn TrophyDescription">{trophy.trophy_desc}</div>
              </div>
            ))}
          </div>
        </div>
      ) : (
        <div>
        Loading...
        </div>
      )}
    </Page>
  );
}
