import { FC } from 'react';
import { Link } from 'react-router-dom';
import Tilt from 'react-parallax-tilt';
import {
  Card,
  Text,
  Flex,
  Progress,
  Grid,
  Box,
  Badge,
  DropdownMenu,
  IconButton,
  Tooltip,
} from '@radix-ui/themes';
import { Trophy, MoreVertical, Crown, ArrowRightLeft, ShieldCheck } from 'lucide-react';
import { webAPIUrl } from './AppSettings';
import { useAuth } from './AuthContext';
import { GameListData } from './GameListData';


interface Props {
  userName: string;
  game: GameListData;
  percent: number;
  isOwner: boolean | null;
  canManage: boolean | null;
  onTransfer?: (game: GameListData) => void;
}

export const GameDisplay: FC<Props> = ({ userName, game, percent, isOwner, canManage, onTransfer }) => {
  const { user } = useAuth();

  return (


<Box style={{ position: 'relative', width: '510px' }}>
  {/* 1. FLOATING IMAGE - 368px wide */}
  <Box
    style={{
      position: 'absolute',
      top: '-50px',
      left: '50%',
      transform: 'translateX(-50%)',
      zIndex: 10,
    }}
  >
    <Tilt
      tiltMaxAngleX={25} 
      tiltMaxAngleY={25}
      transitionSpeed={400}
      scale={1.2}
      glareEnable={true}
      glareMaxOpacity={0.15}
      glareBorderRadius="12px"
    >
      <Link to={`/game/${userName}/${game.shortname}`}>
        <img
          src={`${webAPIUrl}/gamerzilla/game/image/show?game=${game.shortname}`}
          alt={game.name}
          style={{
            objectFit: 'cover',
            borderRadius: '16px',
            boxShadow: 'var(--shadow-6)',
            border: '1px solid rgba(255,255,255,0.1)',
          }}
        />
      </Link>
    </Tilt>
  </Box>

  {/* 2. BACKING CARD */}
  <Card
    size="3"
    variant="surface"
    style={{
      width: '510px',
      paddingTop: '150px', 
      borderRadius: '24px',
      boxShadow: 'var(--shadow-5)',
      overflow: 'visible',
    }}
  >
    {/* 3. FLANKING ICONS - Pushed further to the edges */}
    <Box
      style={{
        position: 'absolute',
        top: '45px',
        left: '20px', 
        zIndex: 15,
      }}
    >
      {isOwner ? (
        <Tooltip content="You own this game">
          <IconButton size="3" variant="soft" color="green" radius="full">
            <ShieldCheck size={22} />
          </IconButton>
        </Tooltip>
      ) : canManage && !game.ownerId ? (
        <Tooltip content="Unclaimed Game">
          <IconButton size="3" variant="soft" color="gray" radius="full">
            <Crown size={22} />
          </IconButton>
        </Tooltip>
      ) : null}
    </Box>

    <Box
      style={{
        position: 'absolute',
        top: '45px',
        right: '20px', 
        zIndex: 15,
      }}
      onClick={(e) => {
        e.preventDefault();
        e.stopPropagation();
      }}
    >
      {canManage && (
        <DropdownMenu.Root>
          <DropdownMenu.Trigger>
            <IconButton size="3" variant="soft" color="gray" highContrast radius="full">
              <MoreVertical size={22} />
            </IconButton>
          </DropdownMenu.Trigger>
          <DropdownMenu.Content align="end">
            <DropdownMenu.Item color="red" onClick={() => onTransfer?.(game)}>
              <ArrowRightLeft size={14} /> Transfer Ownership
            </DropdownMenu.Item>
          </DropdownMenu.Content>
        </DropdownMenu.Root>
      )}
    </Box>

    {/* 4. CONTENT SECTION */}
    <Flex direction="column" gap="1" px="5">
      <Flex justify="between" align="center">
        <Text size="5" weight="bold" truncate style={{ letterSpacing: '-0.02em' }}>
          {game.name}
        </Text>
        {percent === 100 && (
          <Badge color="amber" variant="solid" radius="full" size="2">
            <Trophy size={14} style={{ marginRight: 4 }} /> 100%
          </Badge>
        )}
      </Flex>

      <Box mt="1">
        <Flex justify="between">
          <Text size="1" color="gray" weight="bold">
            Progress
          </Text>
          <Text size="1" color="gray">
            {game.earned} / {game.total}
          </Text>
        </Flex>
        <Progress
          value={percent}
          size="2"
          color={percent === 100 ? 'amber' : 'indigo'}
          style={{ height: '6px', marginTop: '4px' }}
        />
      </Box>
    </Flex>
  </Card>
</Box>
  );
};
