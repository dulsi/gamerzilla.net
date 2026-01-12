import { webAPIUrl } from './AppSettings';

export interface HttpRequest<REQB> {
  path: string;
  method?: string;
  body?: REQB;
  accessToken?: string;
}

export interface HttpResponse<RESB> extends Response {
  parsedBody?: RESB;
}

export const http = async <REQB, RESB>(config: HttpRequest<REQB>): Promise<HttpResponse<RESB>> => {
  const request = new Request(`${webAPIUrl}${config.path}`, {
    method: config.method?.toUpperCase() || 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: config.body ? JSON.stringify(config.body) : undefined,
  });

  if (config.accessToken) {
    request.headers.set('Authorization', `Bearer ${config.accessToken}`);
  }

  try {
    const res: HttpResponse<RESB> = await fetch(request);
    const contentType = res.headers.get('Content-Type') || '';
    let parsedBody: RESB | undefined;

    
    if (contentType.includes('json')) {
      parsedBody = await res.json();
    }
    
    else {
      const text = await res.text();
      
      if (text) {
        
        
        parsedBody = text as unknown as RESB;
      }
    }

    res.parsedBody = parsedBody;

    if (!res.ok) {
      throw res;
    }

    return res;
  } catch (err) {
    console.error('HTTP request failed:', err);
    throw err;
  }
};