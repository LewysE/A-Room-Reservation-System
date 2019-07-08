import json

from PyQt5.QtWidgets import QWidget, QLabel, QApplication, QPushButton, QHBoxLayout, QVBoxLayout, QComboBox, QMainWindow
from googleapiclient.discovery import build
from functools import partial
from httplib2 import Http
from oauth2client import file, client, tools
import sys, datetime, threading
from pathlib import Path
from CalendarApp import CalendarApp


class Select(QMainWindow):

    def __init__(self):

        super().__init__()
        SCOPES = 'https://www.googleapis.com/auth/calendar'

        store = file.Storage('token.json')
        creds = store.get()
        if not creds or creds.invalid:
            flow = client.flow_from_clientsecrets('credentials.json', SCOPES)
            creds = tools.run_flow(flow, store)
        self.service = build('calendar', 'v3', http=creds.authorize(Http()))

        file_path = Path("CalendarId.txt")

        # if a room has already been selected, get the calendar id then
        # load up the main program
        if file_path.is_file():
            f = open("CalendarId.txt", "r")
            calendar_id = f.read()
            self.dialog = QMainWindow()
            self.program = CalendarApp(self.dialog, calendar_id, self)
            self.dialog.show()
            self.hide()

        # if a calendar hasn't been selected get a list of all rooms/calendars
        else:
            page_token = None
            while True:
                self.calendar_list = self.service.calendarList()\
                    .list(pageToken=page_token).execute()
                page_token = self.calendar_list.get('nextPageToken')
                if not page_token:
                    break

            self.init_ui()

    def init_ui(self):
        lbl1 = QLabel()
        lbl1.setText("does this work")

        btn_ok = QPushButton("OK")
        btn_ok.clicked.connect(self.open_window)
        btn_cancel = QPushButton("Cancel")
        btn_cancel.clicked.connect(self.close_window)

        self.combo_box = QComboBox()
        for calendar_list_entry in self.calendar_list['items']:
            try:
                temp = json.loads(calendar_list_entry['description'])
            except:
                print("")
            else:
                test = temp['building'] + " Room " + calendar_list_entry['summary']
                self.combo_box.addItem(test)

        hbox = QHBoxLayout()
        hbox.addStretch(10)
        hbox.addWidget(btn_ok)
        hbox.addWidget(btn_cancel)
        hbox.addWidget(self.combo_box)

        vbox = QVBoxLayout()
        vbox.addStretch(1)
        vbox.addLayout(hbox)

        self.setCentralWidget(QWidget(self))
        self.centralWidget().setLayout(vbox)

        self.setGeometry(300, 300, 300, 150)
        self.setWindowTitle('Buttons')
        self.show()

    def open_window(self):
        for calendar_list_entry in self.calendar_list['items']:
            try:
                temp = json.loads(calendar_list_entry['description'])
            except:
                print("")
            else:
                test =temp['building'] + " Room " + calendar_list_entry['summary']
                if str(self.combo_box.currentText()) == test:
                    f = open("CalendarId.txt", "w+")
                    f.write(calendar_list_entry['id'])
                    calendarid = calendar_list_entry['id']
                    f.close()

        self.dialog = QMainWindow()
        self.program = CalendarApp(self.dialog, calendarid, self)
        self.dialog.show()
        self.hide()

    def close_window(self):
        self.close()




