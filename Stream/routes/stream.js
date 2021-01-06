/* eslint-disable camelcase */
const express = require('express');
const fs = require('fs');
const router = express.Router();
const path = require('path');
const { cropVideo } = require('../helpers/storage');
const videoFileExtension = '.webm';
const ffmpegPath = require('@ffmpeg-installer/ffmpeg').path;
const ffmpeg = require('fluent-ffmpeg');

ffmpeg.setFfmpegPath(ffmpegPath);

/* GET home page. */
router.get(
  '/crop/:videoId/:startTime/:duration',
  async function (req, res, next) {
    const { videoId, startTime, duration } = req.params;

    const videoPath = path.join(
      __dirname,
      '../uploads',
      `${videoId}${videoFileExtension}`
    );
    if (!fs.existsSync(videoPath)) {
      res.status(400).send({ message: "Data don't exist" });
    } else {
      const cropPath = path.join(
        __dirname,
        '../uploads',
        'crops',
        `${videoId}_${startTime}_${duration}${videoFileExtension}`
      );

      if (!fs.existsSync(cropPath)) {
        await ffmpeg(videoPath)
          .setStartTime(startTime)
          .setDuration(duration)
          .output(cropPath)
          .on('end', async function (err) {
            if (!err) {
              console.log('conversion Done');
              await cropVideo(videoId, startTime, cropPath, duration);
              res.json({
                path: cropPath,
                src: `http://192.168.1.102:7002/stream/download?src=${cropPath}`,
                startTime,
                duration
              });
            }
          })
          .on('error', function (err) {
            console.log('error: ', err);
          })
          .run();
      } else {
        return res.json({
          path: cropPath,
          startTime,
          duration,
          src: `http://192.168.1.102:7002/stream/download?src=${cropPath}`
        });
      }
    }
  }
);
router.get('/videos', async function (req, res, next) {
  const videoPath = path.join(__dirname, '../uploads');
  fs.readdirSync(videoPath).forEach((file) => {
    console.log(file);
  });

  res.send({ src: videoPath });
});
router.get('/video/:videoId', async function (req, res, next) {
  const { videoId } = req.params;
  const videoPath = path.join(
    __dirname,
    '../uploads',
    `${videoId}${videoFileExtension}`
  );
  res.send({ path: videoPath, src: `http://192.168.1.102:7002/stream/download?src=${videoPath}` });
});

router.get('/download', async function (req, res, next) {
  const { src } = req.query;
  try {
    res.download(src);
  } catch (error) {
    res.status(400).json(error);
  }
}
);

module.exports = router;
