/* eslint-disable camelcase */
const express = require('express');
const axios = require('axios');
const router = express.Router();

/* GET home page. */
router.get('/', async function (req, res, next) {
  try {
    const { data: generateTokenData } = await axios.post(
      'https://sandbox.api.video/auth/api-key',
      {
        apiKey: 'pLU30zU8WFoUuqfxxMM2CULS8l3oq2zlm0LkBCyngkR'
      }
    );
    const { access_token } = generateTokenData;

    const { data } = await axios.post(
      ' https://sandbox.api.video/live-streams',
      {
        name: 'newStream',
        record: true
      },
      {
        headers: {
          'Content-Type': 'application/json',
          Authorization: 'Bearer ' + access_token // the token is a variable which holds the token
        }
      }
    );
    return res.status(200).json(data);
  } catch (error) {
    return res.error(error);
  }
});

module.exports = router;
