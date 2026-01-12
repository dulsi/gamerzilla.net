import { useEffect, useState, FC } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Tilt from 'react-parallax-tilt'; 
import {
  Card,
  Flex,
  Text,
  Heading,
  Box,
  Avatar,
  Badge,
  Container,
  Progress,
  IconButton,
  Spinner,
  Button,
} from '@radix-ui/themes';
import { getGame, GameData } from './GameListData';
import { webAPIUrl } from './AppSettings';
import { PageTitle } from './PageTitle';
import { ChevronLeft } from 'lucide-react';

export const GamePage: FC = () => {
  const { userName, gameId } = useParams();
  const [game, setGame] = useState<GameData | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

      const goBack = () => {
    
    if (window.history.state && window.history.state.idx > 0) {
      navigate(-1);
    } else {
      
      
      const newPath = '/' + pathSegments.slice(0, -1).join('/');
      navigate(newPath || '/');
    }
  };

  useEffect(() => {
    const fetchGame = async () => {
      if (userName && gameId) {
        const data = await getGame(userName, gameId);
        setGame(data);
        setLoading(false);
      }
    };
    fetchGame();
  }, [userName, gameId]);

  if (loading || !game)
    return (
      <Flex justify="center" p="5">
        <Spinner size="3" />
      </Flex>
    );

  const unlockedCount = game.trophy.filter((t) => t.achieved === '1').length;
  const totalCount = game.trophy.length;
  const percent = totalCount > 0 ? (unlockedCount / totalCount) * 100 : 0;
  const isMastered = totalCount > 0 && unlockedCount === totalCount;
  const pathSegments = location.pathname.split('/').filter(Boolean);
  const isDeep = pathSegments.length > 0;




  return (
    <Box>
      {/* 1. HERO SECTION */}

      <Box
        style={{
          position: 'relative',
          height: '400px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          overflow: 'hidden',
          borderBottom: '1px solid var(--gray-a4)',
          backgroundColor: 'var(--gray-2)',
        }}
      >
        {/* Background Layers */}
        <Box
          style={{
            backgroundImage: `url(${webAPIUrl}/gamerzilla/game/image/show?game=${game.shortname})`,
            backgroundSize: 'cover',
            backgroundPosition: 'center',
            filter: 'blur(60px) saturate(200%)',
            opacity: 0.3,
            position: 'absolute',
            inset: 0,
            zIndex: 0,
          }}
        />
        <Box
          style={{
            position: 'absolute',
            inset: 0,
            zIndex: 1,
            borderRadius: 'var(--radius-5)',
            background:
              'radial-gradient(circle at center, transparent 0%, var(--color-background) 100%)',
          }}
        />

        {/* Hero Content */}
        <Container size="2" style={{ position: 'relative', zIndex: 2 }}>
          <Flex direction="column" align="center" justify="center" gap="4">
            {/* Main Game Image - Native Size */}

            <Tilt
              tiltMaxAngleX={25} 
              tiltMaxAngleY={25}
              transitionSpeed={400}
              scale={1.2}
              glareEnable={true}
              glareMaxOpacity={0.15}
              glareBorderRadius="35px"
              glarePosition='top'
            >
              <img
                src={`${webAPIUrl}/gamerzilla/game/image/show?game=${game.shortname}`}
                alt={game.name}
                style={{
                  maxWidth: '100%', 
                  height: 'auto', 
                  borderRadius: '16px',
                  boxShadow: 'var(--shadow-6)',
                  border: '1px solid rgba(255,255,255,0.1)',
                }}
              />
            </Tilt>
            {/* Stats */}
            <Flex direction="column" align="center" gap="2">
              <Flex direction="row" align="center" gap="2">
                {isDeep ? (
                  <Button
                    variant="ghost"
                    color="gray"
                    onClick={goBack}
                    style={{
                      cursor: 'pointer',
                    }}
                    highContrast
                  >
                    <ChevronLeft size={36} style={{ opacity: 0.8 }} />{' '}
                    <Heading
                      size="8" 
                      weight="bold"
                      align="center"
                      style={{ textShadow: '0 4px 12px rgba(0,0,0,0.5)' }}
                    >
                      {game.name}
                    </Heading>
                  </Button>
                ) : (
                  <Heading
                    size="8" 
                    weight="bold"
                    align="center"
                    style={{ textShadow: '0 4px 12px rgba(0,0,0,0.5)' }}
                  >
                    {game.name}
                  </Heading>
                )}
              </Flex>
              <Flex align="center" gap="3">
                <Badge size="2" variant="surface" color="gray">
                  {unlockedCount} / {totalCount} Trophies
                </Badge>
                {percent === 100 && (
                  <Badge size="2" color="amber" variant="solid">
                    Mastered
                  </Badge>
                )}
              </Flex>
            </Flex>
          </Flex>
        </Container>
      </Box>

      {/* 2. ACHIEVEMENT LIST */}

      <Container size="2" py="6">
        <Flex
          direction="column"
          gap="3"
          style={{
            boxShadow: 'var(--shadow-6)',
            padding: '20px',
            borderRadius: 'var(--radius-4)',
          }}
        >
          {game.trophy.map((trophy) => {
            const isUnlocked = trophy.achieved === '1';

            
            const currentProgress = parseInt(trophy.progress || '0');
            const maxProgress = parseInt(trophy.max_progress || '0');
            const hasProgress = maxProgress > 0;
            const progressPercent = hasProgress ? (currentProgress / maxProgress) * 100 : 0;

            return (
              <Card
                key={trophy.trophy_name}
                variant={isUnlocked ? 'surface' : 'ghost'}
                style={{
                  opacity: isUnlocked ? 1 : 0.7,
                  transition: 'transform 0.2s',
                }}
              >
                <Flex align="start" gap="4">
                  {/* Icon */}
                  <Avatar
                    size="5"
                    src={`${webAPIUrl}/gamerzilla/trophy/image/show?game=${game.shortname}&trophy=${trophy.trophy_name}&achieved=${trophy.achieved}`}
                    fallback="?"
                    radius="large"
                    style={{
                      marginTop: '4px', 
                      filter: isUnlocked ? 'none' : 'grayscale(100%)',
                      border: isUnlocked ? '2px solid var(--accent-9)' : '1px solid var(--gray-6)',
                    }}
                  />

                  {/* Content Column */}
                  <Box style={{ flexGrow: 1 }}>
                    {/* Header Row */}
                    <Flex justify="between" align="center" mb="1">
                      <Heading size="4" color={isUnlocked ? undefined : 'gray'}>
                        {trophy.trophy_name}
                      </Heading>
                      {isUnlocked ? (
                        <Badge color="green" variant="soft">
                          Unlocked
                        </Badge>
                      ) : (
                        <Badge color="gray" variant="outline">
                          Locked
                        </Badge>
                      )}
                    </Flex>

                    {/* Description */}
                    <Text as="div" size="2" color="gray" mb={hasProgress ? '3' : '0'}>
                      {trophy.trophy_desc}
                    </Text>

                    {/* PROGRESS BAR SECTION (Only if max_progress > 0) */}
                    {hasProgress && (
                      <Box>
                        <Flex justify="between" mb="1">
                          <Text size="1" color="gray" weight="bold">
                            Progress
                          </Text>
                          <Text size="1" color="gray">
                            {currentProgress} / {maxProgress}
                          </Text>
                        </Flex>
                        <Progress
                          value={progressPercent}
                          size="2"
                          
                          color={isUnlocked ? 'green' : 'indigo'}
                        />
                      </Box>
                    )}
                  </Box>
                </Flex>
              </Card>
            );
          })}
        </Flex>
      </Container>
    </Box>
  );
};
