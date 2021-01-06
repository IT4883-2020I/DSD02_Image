import React, { useCallback, useEffect, useRef, useState } from 'react';
import { WS_STREAM_HOST } from '../../configs';
import './styles.scss';

let CONFERENCE_ID = 'conference-one';

const DronePage = () => {
  const refVideo = useRef(null);
  const recorder = useRef(null);
  const connection = useRef(null);
  const peer = useRef(null);
  const [offerCreated, setOfferCreated] = useState(false);
  const [recordable, setRecordable] = useState(false);
  const [selfCandidates, setSelfCandidates] = useState([]);
  const [videoCounter, setVideoCounter] = useState(0);

  function createRTCOffer() {
    let rtcConstraints = {
      OfferToReceiveAudio: true,
      OfferToReceiveVideo: true
    };
    peer.current.createOffer(
      function (sessionDescription) {
        peer.current.setLocalDescription(
          sessionDescription,
          function () {
            let message = {
              sessionDescription: sessionDescription,
              type: 'offer'
            };
            connection.current.send(JSON.stringify(message));
            setRecordable(true);
            setOfferCreated(true);
          },
          function (error) {
            console.log('cannot set local description');
            return;
          }
        );
      },
      function (error) {
        console.log(error);
        console.log('cannot create offer');
        return;
      },
      rtcConstraints
    );
  }

  function createRTCAnswer() {
    let rtcConstraints = {
      OfferToReceiveAudio: true,
      OfferToReceiveVideo: true
    };
    peer.current.createAnswer(
      function (sessionDescription) {
        peer.current.setLocalDescription(
          sessionDescription,
          function () {
            let message = {
              sessionDescription: sessionDescription,
              type: 'answer'
            };
            connection.current.send(JSON.stringify(message));
          },
          function (error) {
            console.log('cannot set local description');
          }
        );
      },
      function (error) {
        console.log('cannot create answer');
      },
      rtcConstraints
    );
  }
  // Create websocket
  useEffect(() => {
    let websocketEndpoint = `${WS_STREAM_HOST}/?type=stream`;
    let ws = new WebSocket(websocketEndpoint);
    ws.binaryType = 'arraybuffer';
    connection.current = ws;
  }, []);

  useEffect(() => {
    if (connection.current) {
      connection.current.onmessage = (pureMessage) => {
        let message = JSON.parse(pureMessage.data);
        console.log('message', message);
        if (!offerCreated && message.type === 'offer') {
          console.log('got offer');
          let sessionDescription = new RTCSessionDescription(
            message.sessionDescription
          );
          peer.current.setRemoteDescription(
            sessionDescription,
            createRTCAnswer,
            function (error) {
              console.log('cannot set remote description');
            }
          );
        } else if (offerCreated && message.type === 'answer') {
          console.log('got answer', peer.current);
          let sessionDescription = new RTCSessionDescription(
            message.sessionDescription
          );
          if (peer.current.signalingState === 'stable') {
            console.log('already set remote description');
          } else {
            peer.current.setRemoteDescription(
              sessionDescription,
              function () {
                console.log('created remote description');
              },
              function (error) {
                console.log('cannot set remote description', error);
              }
            );
          }
        } else if (message.type === 'candidate') {
          if (selfCandidates.indexOf(message.candidate) === -1) {
            let candidate = new RTCIceCandidate({
              sdpMLineIndex: message.label,
              candidate: message.candidate
            });
            peer.current.addIceCandidate(candidate);
          }
        } else if (message.type === 'start-recording') {
          console.log('starting to record');
          recorder.current.start(3000);
        } else if (message.type === 'stop-recording') {
          console.log('stop recording');
          recorder.current.stop();
        } else if (
          message.part === videoCounter &&
          recorder.current.state === 'inactive'
        ) {
          console.log('sending over complete video message');
          connection.current.send(
            JSON.stringify({
              completedVideo: message.fileName,
              id: CONFERENCE_ID
            })
          );
        }
      };
    }
  }, [offerCreated, selfCandidates, videoCounter]);

  // Create Local stream

  const getRecorder = useCallback((stream) => {
    let options = { mimeType: 'video/webm' };
    recorder.current = new MediaRecorder(stream, options);
    recorder.current.ondataavailable = (event) => {
      let reader = new FileReader();
      reader.readAsArrayBuffer(event.data);
      setVideoCounter((state) => state + 1);
      reader.onloadend = function (event) {
        console.log(reader.result);
        connection.current.send(reader.result);
      };
    };
  }, []);

  const createPeerConnection = useCallback((ws, mediaStream) => {
    let rtcConfig = {
      iceServers: [{ urls: ['stun:stun.l.google.com:19302'] }]
    };
    peer.current = new RTCPeerConnection(rtcConfig);
    peer.current.addStream(mediaStream);
    peer.current.onicecandidate = (event) => {
      console.log('onicecandidate event: ');
      if (event.candidate) {
        ws.send(
          JSON.stringify({
            type: 'candidate',
            label: event.candidate.sdpMLineIndex,
            id: event.candidate.sdpMid,
            candidate: event.candidate.candidate
          })
        );
        setSelfCandidates((state) => state.concat(event.candidate.candidate));
      } else {
        console.log('end of candidates');
      }
    };
    peer.current.onaddstream = (event) => {
      console.log('onaddstream', event.stream);
      ws.send(
        JSON.stringify({
          id: CONFERENCE_ID
        })
      );
    };
    peer.current.onremovestream = () => {
      console.log('handleRemoteStreamRemoved');
    };
  }, []);

  useEffect(() => {
    let config = { video: true, audio: true };
    navigator.mediaDevices.getUserMedia(config).then(function (mediaStream) {
      refVideo.current.srcObject = mediaStream;
      if (connection.current && connection.current.readyState === 1) {
        getRecorder(mediaStream);
        createPeerConnection(connection.current, mediaStream);
      }
    });
  }, [createPeerConnection, getRecorder]);

  const onStartRecord = () => {
    if (connection.current && recordable) {
      setRecordable(false);
      connection.current.send(JSON.stringify({ type: 'start-recording' }));
    }
  };
  const onStopRecord = () => {
    if (connection.current) {
      connection.current.send(JSON.stringify({ type: 'stop-recording' }));
    }
  };

  return (
    <div className="drone-page">
      <div className="header">
        <div className="title">Drone page</div>
      </div>
      <div className="container">
        <div className="sites video">
          <video autoPlay ref={refVideo} />
        </div>
        <div className="sites actions">
          <div className="button" onClick={createRTCOffer}>
            Initial stream
          </div>
          <div className="button" onClick={onStartRecord}>
            Start recording
          </div>
          <div className="button" onClick={onStopRecord}>
            Stop recording
          </div>
        </div>
      </div>
    </div>
  );
};

export default DronePage;
