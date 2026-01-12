import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { Box, Card, Text, Flex, Button, Spinner } from '@radix-ui/themes';
import { CheckCircle, XCircle } from 'lucide-react';
import { http } from './http';
import { useAuth } from './AuthContext';
import { TokenResult } from './models/TokenResult';

export const VerifyAccountPage = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const { user: loggedInUser } = useAuth();

  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [data, setData] = useState<TokenResult | null | undefined>(null);
  const [errorMsg, setErrorMsg] = useState('');

  useEffect(() => {
    const verify = async () => {
      try {
        const res = await http<undefined, TokenResult>({
          path: `/user/verify?token=${token}`,
          method: 'GET',
        });

        setStatus('success');
        setData(res.parsedBody);
      } catch (err: any) {
        setStatus('error');
        setErrorMsg(err.parsedBody || 'Verification failed.');
      }
    };
    if (token) verify();
  }, [token]);

  const isTransfer = data?.actionType === 'Transfer';
  const isRegistration = data?.actionType === 'Registration';
  const targetPath = isTransfer ? `/games/${loggedInUser?.userName || ''}` : '/signin';

  return (
    <Flex align="center" justify="center" mt="5">
      <Card size="4" style={{ maxWidth: 500, width: '100%', textAlign: 'center' }}>
        {status === 'loading' && (
          <Flex
            direction="column"
            align="center"
            justify="center"
            gap="4"
            style={{ minHeight: '200px' }}
          >
            <Spinner size="3" />
            <Text size="2" color="gray">
              Verifying your account...
            </Text>
          </Flex>
        )}

        {status === 'success' && data && (
          <Flex direction="column" align="center" gap="4">
            <CheckCircle size={64} color="green" />
            <Text size="5" weight="bold">
              {isTransfer ? 'Transfer Accepted!' : 'Verified!'}
            </Text>
            <Text color="gray">{data.message}</Text>
            <Link to={targetPath}>
              <Button size="3" mt="4" variant="surface">
                {isTransfer ? 'View My Library' : 'Go to Login'}
              </Button>
            </Link>
          </Flex>
        )}

        {status === 'error' && (
          <Flex direction="column" align="center" gap="4">
            <XCircle size={64} color="red" />
            <Text size="5" weight="bold">
              Verification Failed
            </Text>
            <Text color="red">{errorMsg}</Text>
            <Button variant="surface" mt="4" onClick={() => (window.location.href = '/')}>
              Back to Home
            </Button>
          </Flex>
        )}
      </Card>
    </Flex>
  );
};
