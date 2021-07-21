import { FC } from 'react';
import { Link } from 'react-router-dom';
import { GameListData } from './GameListData';
import { relativeAPIUrl } from './AppSettings';
import './GameList.css';

interface Props {
  userName: string
  data: GameListData[];
}
export const GameList: FC<Props> = ({userName, data}) => (
  <div className="GameList">
    <div className="GameListItem GameListRow">
      <div className="GameListColumn GameListTitle">Name</div>
      <div className="GameListColumn GameListEarned">Earned</div>
      <div className="GameListColumn GameListTotal">Total</div>
    </div>
    {data.map(game => (
      <div key={game.shortname} className="GameListItem GameListRow">
        <div className="GameListColumn GameListTitle"><Link to={`/game/${userName}/${game.shortname}`}><img alt={`${game.name}`} src={`${relativeAPIUrl}/game/image/show?game=${game.shortname}`} />{game.name}</Link></div>
        <div className="GameListColumn GameListEarned">{game.earned}</div>
        <div className="GameListColumn GameListTotal">{game.total}</div>
      </div>
    ))}
  </div>
);