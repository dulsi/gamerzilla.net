import { useEffect, useState, useMemo, FC, FormEvent } from 'react';
import { useNavigate, useSearchParams, Link as RouterLink } from 'react-router-dom';
import { defaultServerOptions, getServerOptions, requestAuthToken, ServerOptions } from './User'; 
import {
  Box,
  Card,
  Flex,
  Heading,
  TextField,
  Button,
  Text,
  Callout,
  Spinner,
  Progress,
} from '@radix-ui/themes';
import { Info, CheckCircle, Mail } from 'lucide-react';
import { getPasswordStrength } from './PasswordStrength';
import { http } from './http';

export const ResetPasswordPage: FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');

  
  const [email, setEmail] = useState('');
  const [requestSent, setRequestSent] = useState(false);
  const [options, setOptions] = useState<ServerOptions>(defaultServerOptions);

    useEffect(() => {
      getServerOptions().then(setOptions);
    }, []);

  
  const [password, setPassword] = useState('');
  const [passwordVerify, setPasswordVerify] = useState('');

  
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  
  const strength = useMemo(() => getPasswordStrength(password), [password]);
  const doPasswordsMatch = !passwordVerify || password === passwordVerify;
  const isEmailValid = useMemo(() => {
    const emailRegex = /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i;

    if (!options.emailEnabled) return true; 
    return email.trim() !== '' && emailRegex.test(email);
  }, [email, options.emailEnabled]);
  const isResetFormValid = token && password !== '' && doPasswordsMatch && strength.score >= 2;


  

  
  const handleRequestLink = async (e: FormEvent) => {
    e.preventDefault();
    if (!email) return;

    setError(null);
    setIsLoading(true);
    try {
      await requestAuthToken(email);
      setRequestSent(true);
    } catch (err: any) {
      setError("We couldn't process your request. Please try again later.");
    } finally {
      setIsLoading(false);
    }
  };
const handleResetSubmit = async (e: FormEvent) => {
  e.preventDefault();
  if (!isResetFormValid) return;

  setError(null);
  setIsLoading(true);

  try {
    
    const res = await http<any, { message: string }>({
      path: '/account/reset-password', 
      method: 'POST',
      body: { token, newPassword: password },
    });

    setIsSuccess(true);
    setTimeout(() => navigate('/signin'), 3000);
  } catch (err: any) {
    
    setError(err.parsedBody || 'Failed to reset password.');
  } finally {
    setIsLoading(false);
  }
};

  const RequiredMark = () => (
    <Text color="red" style={{ marginLeft: '2px' }}>
      *
    </Text>
  );

  

  
  if (isSuccess) {
    return (
      <Box maxWidth="400px" mx="auto" pt="8">
        <Card size="4" variant="surface" style={{ borderRadius: '16px' }}>
          <Flex direction="column" gap="4" align="center" py="4">
            <CheckCircle size={48} color="var(--green-9)" />
            <Heading size="6">Password Updated</Heading>
            <Text size="2" color="gray" align="center">
              Your password has been reset successfully. Redirecting to Sign In...
            </Text>
            <Button variant="surface" asChild mt="2">
              <RouterLink to="/signin">Go to Sign In</RouterLink>
            </Button>
          </Flex>
        </Card>
      </Box>
    );
  }

  
  if (!token) {
    return (
      <Box maxWidth="400px" mx="auto" pt="8">
        <Card
          size="4"
          style={{
            borderRadius: '16px',
            boxShadow: 'var(--shadow-6)',
            border: '1px solid rgba(255,255,255,0.1)',
          }}
        >
          {requestSent ? (
            <Flex direction="column" gap="4" align="center">
              <Mail size={48} color="var(--green-9)" />
              <Heading size="5">Check your email</Heading>
              <Text align="center" color="gray">
                If an account exists for <strong>{email}</strong>, you will receive a password reset
                link shortly.
              </Text>
              <Button variant="outline" onClick={() => setRequestSent(false)}>
                Back
              </Button>
            </Flex>
          ) : (
            <form onSubmit={handleRequestLink}>
              <Flex direction="column" gap="4">
                <Heading size="6" align="center">
                  Forgot Password
                </Heading>
                <Text size="2" color="gray" align="center">
                  Enter your email and we'll send you a link to reset your password.
                </Text>

                {error && (
                  <Callout.Root color="red">
                    <Callout.Icon>
                      <Info size={16} />
                    </Callout.Icon>
                    <Callout.Text>{error}</Callout.Text>
                  </Callout.Root>
                )}

                <label>
                  <Text as="div" size="2" mb="1" weight="bold">
                    Email Address <RequiredMark />
                  </Text>
                  <TextField.Root
                    size="3"
                    type="email"
                    placeholder="Enter your email address"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    disabled={isLoading}
                    color={email.length > 0 && !isEmailValid ? 'red' : undefined} 
                    required
                  />
                </label>

                {email.length > 0 && !isEmailValid && (
                  <Text color="red" size="1">
                    Please enter a valid email address (e.g., name@example.com)
                  </Text>
                )}

                <Button size="3" disabled={isLoading || !isEmailValid}>
                  <Spinner loading={isLoading}>Send Reset Link</Spinner>
                </Button>

                <Text size="2" align="center">
                  <RouterLink
                    to="/signin"
                    style={{ color: 'var(--accent-11)', textDecoration: 'none' }}
                  >
                    Back to Sign In
                  </RouterLink>
                </Text>
              </Flex>
            </form>
          )}
        </Card>
      </Box>
    );
  }

  
  return (
    <Box maxWidth="400px" mx="auto" pt="8">
      <Card size="4" style={{ borderRadius: '16px' }}>
        <form onSubmit={handleResetSubmit}>
          <Flex direction="column" gap="5">
            <Heading as="h2" size="6" align="center">
              New Password
            </Heading>

            {error && (
              <Callout.Root color="red">
                <Callout.Icon>
                  <Info size={16} />
                </Callout.Icon>
                <Callout.Text>{error}</Callout.Text>
              </Callout.Root>
            )}

            <Flex direction="column" gap="4">
              <Box>
                <label>
                  <Text as="div" size="2" mb="1" weight="bold">
                    New Password <RequiredMark />
                  </Text>
                  <TextField.Root
                    size="3"
                    type="password"
                    placeholder="••••••••"
                    value={password}
                    onChange={(e) => setPassword(e.currentTarget.value)}
                    disabled={isLoading}
                  />
                </label>
                {password.length > 0 && (
                  <Box mt="2">
                    <Flex justify="between" mb="1">
                      <Text size="1" color={strength.color}>
                        {strength.label}
                      </Text>
                      <Text size="1" color="gray">
                        Min. 'Fair'
                      </Text>
                    </Flex>
                    <Progress value={strength.score * 33.3} color={strength.color} size="1" />
                  </Box>
                )}
              </Box>

              <Box>
                <label>
                  <Text as="div" size="2" mb="1" weight="bold">
                    Confirm New Password <RequiredMark />
                  </Text>
                  <TextField.Root
                    size="3"
                    type="password"
                    placeholder="••••••••"
                    value={passwordVerify}
                    onChange={(e) => setPasswordVerify(e.currentTarget.value)}
                    disabled={isLoading}
                    color={!doPasswordsMatch ? 'red' : undefined}
                  />
                </label>
                {!doPasswordsMatch && (
                  <Text color="red" size="1" mt="1">
                    Passwords do not match.
                  </Text>
                )}
              </Box>
            </Flex>

            <Button
              size="3"
              type="submit"
              disabled={isLoading || !isResetFormValid}
              style={{ cursor: isLoading || !isResetFormValid ? 'not-allowed' : 'pointer' }}
            >
              <Spinner loading={isLoading}>Update Password</Spinner>
            </Button>
          </Flex>
        </form>
      </Card>
    </Box>
  );
};
