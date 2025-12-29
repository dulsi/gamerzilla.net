import { ReactNode } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Flex, Heading, IconButton, Box, Button } from '@radix-ui/themes';
import { ChevronLeft } from 'lucide-react';

interface Props {
  children?: ReactNode;
}

export const PageTitle = ({ children }: Props) => {
const navigate = useNavigate();
const location = useLocation();

const pathSegments = location.pathname.split('/').filter(Boolean);
const isDeep = pathSegments.length > 0;

const goBack = () => {
  
  const rootSegment = pathSegments[0]?.toLowerCase();

  
  if (rootSegment === 'games') {
    navigate('/');
    return;
  }

  
  if (window.history.state && window.history.state.idx > 0) {
    navigate(-1);
  } else {
    const newPath = '/' + pathSegments.slice(0, -1).join('/');
    navigate(newPath || '/');
  }
};

  return (
    <Box mb="5">
      <Flex
        align="center"
        gap="1" 
        style={{
          
          
          marginLeft: isDeep ? '-12px' : '0',
          transition: 'margin 0.2s ease',
        }}
      >
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
              size="7"
              weight="bold"
              style={{
                textShadow: `
      0 3px 1px rgba(0,0,0,0.65),
      0 12px 10px rgba(0,0,0,0.45),
      0 0 12px rgba(255,255,255,0.08)
    `,
                transform: 'translateY(-0.5px)',
              }}
            >
              {children}
            </Heading>
          </Button>
        ) : (
          <Heading
            size="7" 
            weight="bold"
          >
            {children}
          </Heading>
        )}
      </Flex>
    </Box>
  );
};
